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
    using Microsoft.AspNetCore.WebUtilities;

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
                await this.nextMiddleware(context);
            }

            var targetRequestMessage = CreateTargetMessage(context, targetUri);
            using (var responseMessage = await HttpClient.SendAsync(
                                             targetRequestMessage,
                                             HttpCompletionOption.ResponseHeadersRead,
                                             context.RequestAborted))
            {
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                CopyFromTargetResponseHeaders(context, responseMessage);
                await ProcessResponseContent(context, responseMessage, targetUri, replace);
            }
        }

        private static async Task ProcessResponseContent(HttpContext context, HttpResponseMessage responseMessage, Uri targetUri, bool replace)
        {
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

                await context.Response.WriteAsync(stringContent, Encoding.UTF8);
            }
            else
            {
                await context.Response.Body.WriteAsync(content);
            }

            bool IsContentOfType(string type)
            {
                return responseMessage?.Content?.Headers?.ContentType?.MediaType?.StartsWith(type) == true;
            }
        }

        private static HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);
            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Headers.Referrer = targetUri;
            requestMessage.Method = new HttpMethod(context.Request.Method);
            return requestMessage;
        }

        private static void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private static void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            context.Response.Headers.Remove("transfer-encoding");
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
