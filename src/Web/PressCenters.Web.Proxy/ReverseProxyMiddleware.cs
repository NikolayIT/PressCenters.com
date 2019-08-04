namespace PressCenters.Web.Proxy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

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
            var targetUri = this.BuildTargetUri(context.Request.Path, out var replace);
            if (targetUri == null)
            {
                await this.nextMiddleware(context);
            }

            var targetRequestMessage = this.CreateTargetMessage(context, targetUri);
            using (var responseMessage = await HttpClient.SendAsync(
                                             targetRequestMessage,
                                             HttpCompletionOption.ResponseHeadersRead,
                                             context.RequestAborted))
            {
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                this.CopyFromTargetResponseHeaders(context, responseMessage);
                await this.ProcessResponseContent(context, responseMessage, replace);
            }
        }

        private async Task ProcessResponseContent(HttpContext context, HttpResponseMessage responseMessage, bool replace)
        {
            var content = await responseMessage.Content.ReadAsByteArrayAsync();

            if (this.IsContentOfType(responseMessage, "text/html")
                || this.IsContentOfType(responseMessage, "text/javascript")
                || this.IsContentOfType(responseMessage, "text/css")
                || this.IsContentOfType(responseMessage, "application/javascript"))
            {
                var stringContent = Encoding.UTF8.GetString(content);
                if (replace)
                {
                    stringContent = stringContent.Replace("https://", "/https/").Replace("http://", "/http/");
                }

                await context.Response.WriteAsync(stringContent, Encoding.UTF8);
            }
            else
            {
                await context.Response.Body.WriteAsync(content);
            }
        }

        private bool IsContentOfType(HttpResponseMessage responseMessage, string type)
        {
            return responseMessage?.Content?.Headers?.ContentType?.MediaType?.StartsWith(type) == true;
        }

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            this.CopyFromOriginalRequestContentAndHeaders(context, requestMessage);
            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = new HttpMethod(context.Request.Method);
            return requestMessage;
        }

        private void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
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

        private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
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

        private Uri BuildTargetUri(PathString pathString, out bool replace)
        {
            replace = true;
            if (pathString.StartsWithSegments("/http", out var remainingHttpPath))
            {
                return new Uri("http:/" + remainingHttpPath);
            }
            else if (pathString.StartsWithSegments("/https", out var remainingHttpsPath))
            {
                return new Uri("https:/" + remainingHttpsPath);
            }
            else if (pathString.StartsWithSegments("/_plain/http", out var remainingHttpPathWithNoReplace))
            {
                replace = false;
                return new Uri("http:/" + remainingHttpPathWithNoReplace);
            }
            else if (pathString.StartsWithSegments("/_plain/https", out var remainingHttpsPathWithNoReplace))
            {
                replace = false;
                return new Uri("https:/" + remainingHttpsPathWithNoReplace);
            }

            return null;
        }
    }
}
