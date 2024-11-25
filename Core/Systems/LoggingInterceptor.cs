using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.HttpLogging;

namespace Core.Systems;

[ExcludeFromCodeCoverage]
public sealed class LoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
            if (logContext.HttpContext.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
            {
                // Don't log anything if the request is for the health endpoint.
                logContext.LoggingFields = HttpLoggingFields.None;
            }

            return default;
        }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
            return default;
        }


}