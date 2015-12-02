using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using org.buraktamturk.aspnet.LetsEncryptMiddleware;

namespace sample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseIISPlatformHandler();

            app.UseLetsEncrypt(new LetsEncryptMiddlewareOptions
            {
                useProxyMode = false,
                proxyPrefix = "https://vpn.buraktamturk.org",

                usePutMode = true,
                preSharedKey = "PRE-SHARED-KEY-HERE",
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
