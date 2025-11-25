using Elzik.Breef.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;
using System.Diagnostics;

namespace Elzik.Breef.Api.ExceptionHandling;

public static class ExceptionHandlingExtensions
{
    public static void AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
    }

    public static void UseExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionHandlerFeature?.Error;
                int statusCode;
                string title;
                string detail;

                if (exception is ICallerFixableException)
                {
                    if (string.IsNullOrWhiteSpace(exception.Message))
                    {
                        throw new InvalidOperationException(
                            "Caller-fixable exception must have a non-empty message for the caller to fix.",
                            exception);
                    }
                    statusCode = StatusCodes.Status400BadRequest;
                    title = "There was a problem with your request";
                    detail = exception.Message;
                }
                else
                {
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "An error occurred while processing your request";
                    detail = "Contact your Breef administrator for a solution.";
                }

                var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail
                };

                if(Activity.Current != null)
                {
                    problemDetails.Extensions["traceId"] = Activity.Current.TraceId.ToString();
                }

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problemDetails);
            });
        });
    }
}
