using Elzik.Breef.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;

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

                if (exception is CallerFixableHttpRequestException callerFixable)
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    title = "There was a problem with your request,";
                    detail = callerFixable.Message;
                }
                else
                {
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "An error occurred while processing your request.";
                    detail = "Contact your Breef administrator for a solution.";
                }

                var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problemDetails);
            });
        });
    }
}
