using System.Diagnostics.CodeAnalysis;

namespace Core.Systems;

[ExcludeFromCodeCoverage]
public class LoggingMiddleware(RequestDelegate next, 
    ILogger<LoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        
        logger.LogDebug("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await next(context);
    }
    
}