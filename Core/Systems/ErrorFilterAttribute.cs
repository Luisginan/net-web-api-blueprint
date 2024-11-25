using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Core.Systems;
[ExcludeFromCodeCoverage]
public class ErrorFilterAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(new
            {
                success = false,
                status_code = 400,
                status = GetReasonPhrase(400),
                message = "Invalid data provided. Please check your input and try again.",
            });
        }
        base.OnResultExecuting(context);
    }
    
    private static string GetReasonPhrase(int statusCode)
    {
        return Enum.GetName(typeof(HttpStatusCode), statusCode) ?? string.Empty;
    }
}