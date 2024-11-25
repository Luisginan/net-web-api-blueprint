using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Core.Systems;

[ExcludeFromCodeCoverage]
public class ResponseFilterAttribute : ActionFilterAttribute, IExceptionFilter
{
    private readonly ActivitySource _activitySource = new("api");

    public void OnException(ExceptionContext context)
    {
        using var activity = _activitySource.StartActivity(ActivityKind.Internal);
        context.HttpContext.Response.StatusCode = 500;
        var ex = context.Exception;

        context.Result = new ObjectResult(new
        {
            success = false,
            status_code = 500,
            status = GetReasonPhrase(500),
            message = context.Exception.Message
        });

        context.HttpContext.Response.StatusCode = 500;
        context.ExceptionHandled = true;

        activity?.SetTag("exception", ex.Message);
        activity?.SetTag("stackTrace", ex.StackTrace);
        activity?.SetTag("exceptionType", ex.GetType().ToString());
        activity?.SetTag("exceptionSource", ex.Source);
        activity?.SetTag("exceptionTargetSite", ex.TargetSite?.ToString());
        activity?.SetTag("exceptionData", ex.Data.ToString());
    }

    private static string GetReasonPhrase(int statusCode)
    {
        return Enum.GetName(typeof(HttpStatusCode), statusCode) ?? string.Empty;
    }

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        
        if (context.Result is ObjectResult { Value: not null } objectResult)
        {
            var success = true;
            if (objectResult.StatusCode != null)
            {
                success = objectResult.StatusCode is < 300 and >= 200;
            }

            var statusCode = objectResult.StatusCode;
            var status = statusCode != null ? GetReasonPhrase(statusCode.Value) : string.Empty;
            var modifiedData = new
            {
                success,
                status_code = statusCode,
                status,
                data = objectResult.Value
            };

            objectResult.Value = modifiedData;
        }

        base.OnResultExecuting(context);
    }
}