using System.Reflection;

namespace AESMovilAPI
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (path.StartsWith("/swagger-ui/custom/"))
            {
                var resourceName = path.Replace("/swagger-ui/custom/", "AESMovilAPI.SwaggerCustomizations.").Replace("/", ".");
                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();
                var resourceStream = assembly.GetManifestResourceStream(resourceName);
                if (resourceStream != null)
                {
                    var extension = Path.GetExtension(path);
                    context.Response.ContentType = extension switch
                    {
                        ".css" => "text/css",
                        ".js" => "application/javascript",
                        ".png" => "image/png",
                        _ => "application/octet-stream"
                    };
                    await resourceStream.CopyToAsync(context.Response.Body);
                    return;
                }
                else
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Resource not found");
                    return;
                }
            }

            await _next(context);
        }
    }
}
