namespace PressCenters.Web.Proxy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Html.Parser;

    using Microsoft.AspNetCore.Http;

    public class ReverseProxyMiddleware
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly RequestDelegate nextMiddleware;

        public ReverseProxyMiddleware(RequestDelegate nextMiddleware)
        {
            this.nextMiddleware = nextMiddleware;
        }

        public async Task Invoke(HttpContext context)
        {
            var targetUri = BuildTargetUri(context.Request.Path, context.Request.QueryString, out var replace);
            if (targetUri == null)
            {
                var referer = context.Request.Headers["Referer"].FirstOrDefault();
                if (referer?.Contains(context.Request.Host.ToString()) == true)
                {
                    var refererUri = new Uri(referer);
                    var refererParts = refererUri.PathAndQuery.Split("/");
                    targetUri = new Uri($"{refererParts[1]}://{refererParts[2]}" + context.Request.Path + context.Request.QueryString);
                }
                else
                {
                    await this.nextMiddleware(context);
                    return;
                }
            }

            var targetRequestMessage = CreateTargetMessage(context.Request, targetUri, replace);
            using (var responseMessage = await HttpClient.SendAsync(
                                             targetRequestMessage,
                                             HttpCompletionOption.ResponseHeadersRead,
                                             context.RequestAborted))
            {
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                await ProcessResponseContent(context.Response, responseMessage, targetUri, replace);
            }
        }

        private static HttpRequestMessage CreateTargetMessage(HttpRequest originalRequest, Uri targetUri, bool replace)
        {
            var stripHeaders = new List<string>();
            if (replace)
            {
                stripHeaders.Add("accept-encoding");
                stripHeaders.Add("content-encoding");
            }

            var requestMessage = new HttpRequestMessage();
            foreach (var header in originalRequest.Headers)
            {
                if (!stripHeaders.Contains(header.Key.ToLower()))
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            if (requestMessage.Headers.Contains("Origin"))
            {
                requestMessage.Headers.Remove("Origin");
                requestMessage.Headers.Add("Origin", targetUri.GetLeftPart(UriPartial.Authority));
            }

            requestMessage.Headers.Referrer = targetUri;
            requestMessage.Method = new HttpMethod(originalRequest.Method);
            return requestMessage;
        }

        private static async Task ProcessResponseContent(HttpResponse response, HttpResponseMessage responseMessage, Uri targetUri, bool replace)
        {
            foreach (var header in responseMessage.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            response.Headers.Remove("transfer-encoding");

            var content = await responseMessage.Content.ReadAsByteArrayAsync();
            if (replace && (IsContentOfType("text/html") || IsContentOfType("text/javascript")
                                                         || IsContentOfType("text/css")
                                                         || IsContentOfType("application/javascript")))
            {
                var stringContent = Encoding.UTF8.GetString(content);
                if (IsContentOfType("text/html"))
                {
                    var parser = new HtmlParser();
                    var document = parser.ParseDocument(stringContent);
                    NormalizeUrlsRecursively(document.DocumentElement, targetUri);
                    stringContent = document.ToHtml();
                }
                else
                {
                    stringContent = stringContent.Replace("https://", "/https/").Replace("http://", "/http/");
                }

                await response.WriteAsync(stringContent, Encoding.UTF8);
            }
            else
            {
                await response.Body.WriteAsync(content);
            }

            bool IsContentOfType(string type)
            {
                return responseMessage?.Content?.Headers?.ContentType?.MediaType?.StartsWith(type) == true;
            }
        }

        private static Uri BuildTargetUri(PathString pathString, QueryString queryString, out bool replace)
        {
            replace = true;
            if (pathString.StartsWithSegments("/http", out var remainingHttpPath))
            {
                return new Uri("http:/" + remainingHttpPath + queryString);
            }

            if (pathString.StartsWithSegments("/https", out var remainingHttpsPath))
            {
                return new Uri("https:/" + remainingHttpsPath + queryString);
            }

            if (pathString.StartsWithSegments("/_plain/http", out var remainingHttpPathWithNoReplace))
            {
                replace = false;
                return new Uri("http:/" + remainingHttpPathWithNoReplace + queryString);
            }

            if (pathString.StartsWithSegments("/_plain/https", out var remainingHttpsPathWithNoReplace))
            {
                replace = false;
                return new Uri("https:/" + remainingHttpsPathWithNoReplace + queryString);
            }

            return null;
        }

        private static void NormalizeUrlsRecursively(IElement element, Uri originalUrl)
        {
            if (element == null)
            {
                return;
            }

            if (element.Attributes["href"] != null)
            {
                element.SetAttribute("href", NormalizeUrl(element.Attributes["href"].Value));
            }

            if (element.Attributes["src"] != null)
            {
                element.SetAttribute("src", NormalizeUrl(element.Attributes["src"].Value));
            }

            foreach (var node in element.Children)
            {
                NormalizeUrlsRecursively(node, originalUrl);
            }

            string NormalizeUrl(string url)
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    return string.Empty;
                }

                if (Uri.TryCreate(originalUrl, url, out var result))
                {
                    url = result.ToString();
                }

                return url.Replace("https://", "/https/").Replace("http://", "/http/");
            }
        }
    }
}
