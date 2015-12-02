using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace org.buraktamturk.aspnet.LetsEncryptMiddleware
{
    public class LetsEncryptMiddlewareOptions {
        public bool useProxyMode { get; set; }

        public string proxyPrefix { get; set; }
        
        public bool usePutMode { get; set; }

        public string preSharedKey { get; set; } // TODO: replace it with RSA key or something...
    }

    public class LetsEncryptMiddlewareChallengeItem {
        public string ContentType;

        public byte[] data;
    }

    public class LetsEncryptMiddlewareCache {
        public Dictionary<string, LetsEncryptMiddlewareChallengeItem> challenges = new Dictionary<string, LetsEncryptMiddlewareChallengeItem>();
    }

    public class AspnetLetsEncryptMiddleware {
        RequestDelegate _next;
        LetsEncryptMiddlewareOptions _options;
        LetsEncryptMiddlewareCache _cache;

        public AspnetLetsEncryptMiddleware(RequestDelegate next, LetsEncryptMiddlewareOptions options, LetsEncryptMiddlewareCache cache)
        {
            _next = next;
            _options = options;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context) {
            if(_options.usePutMode) {
                if (context.Request.Method == "PUT" && context.Request.Query["key"] == _options.preSharedKey) {
                    byte[] data = null;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        await context.Request.Body.CopyToAsync(ms);
                        data = ms.ToArray();
                    }

                    _cache.challenges[context.Request.Path] = new LetsEncryptMiddlewareChallengeItem
                    {
                        ContentType = context.Request.ContentType,
                        data = data
                    };
                } else if(context.Request.Method == "GET") {
                    LetsEncryptMiddlewareChallengeItem data;
                    if(_cache.challenges.TryGetValue(context.Request.Path, out data)) {
                        context.Response.ContentType = data.ContentType ?? "text/plain";
                        context.Response.ContentLength = data.data.Length;
                        await context.Response.Body.WriteAsync(data.data, 0, data.data.Length);
                        await context.Response.Body.FlushAsync();
                    }
                }
            }

            if(_options.useProxyMode) {
                using(var http = new HttpClient())
                using(var response = await http.GetAsync(_options.proxyPrefix + context.Request.Path))
                using(var data = await response.Content.ReadAsStreamAsync())
                {
                    context.Response.ContentType = response.Content.Headers.ContentType.ToString();
                    await data.CopyToAsync(context.Response.Body);
                    return;
                }
            }

            context.Response.StatusCode = 404;
        }
    }

    public static class AspnetLetsEncryptMiddlewareExtensions {
        public static IApplicationBuilder UseLetsEncrypt(this IApplicationBuilder builder, LetsEncryptMiddlewareOptions options) {
            var cache = new LetsEncryptMiddlewareCache();
            return builder.Map("/.well-known", a => a.UseMiddleware<AspnetLetsEncryptMiddleware>(options, cache));
        }
    }
}
