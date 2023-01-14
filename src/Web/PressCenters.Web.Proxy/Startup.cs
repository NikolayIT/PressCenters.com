namespace PressCenters.Web.Proxy
{
    using System;
    using System.Net.Sockets;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System.Net.Http;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Map("/version", (builder) =>
            {
                builder.Run(async (context) =>
                {
                    StringBuilder output = new StringBuilder();
                    output.AppendLine($"ProcessorCount: {Environment.ProcessorCount}");
                    output.AppendLine($"WorkingSet: {Environment.WorkingSet}");
                    output.AppendLine($"OSVersion: {Environment.OSVersion}");
                    output.AppendLine($"Version: {Environment.Version}");
                    string externalIpString = (await new HttpClient().GetStringAsync("http://icanhazip.com")).Trim();
                    output.AppendLine($"IP: {externalIpString}");
                    await context.Response.WriteAsync(output.ToString());
                });
            });
            app.UseMiddleware<ReverseProxyMiddleware>();

            app.Run(context =>
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            });
        }
    }
}
