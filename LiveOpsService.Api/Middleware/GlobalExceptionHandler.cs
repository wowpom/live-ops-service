using LiveOpsService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using LiveOpsService.Models;

namespace LiveOpsService.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            RequestValidationException => StatusCodes.Status400BadRequest,
            BadHttpRequestException badRequest => badRequest.StatusCode,
            IAppException appException => appException.StatusCode,
            _ => StatusCodes.Status500InternalServerError
        };

        ProblemDetails problem = exception is RequestValidationException validation
            ? new HttpValidationProblemDetails(validation.Errors)
            : new ProblemDetails();
        problem.Status = statusCode;
        problem.Title = ReasonPhrases.GetReasonPhrase(statusCode);
        problem.Detail = exception switch
        {
            RequestValidationException => "One or more fields are invalid.",
            BadHttpRequestException => "The request body is missing, malformed or unsupported.",
            IAppException => exception.Message,
            _ => "Internal server error."
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (statusCode >= 500)
        {
            logger.LogError(exception, "Unhandled request error. TraceId: {TraceId}", httpContext.TraceIdentifier);
        }

        await Results.Problem(problem).ExecuteAsync(httpContext);

        return true;
    }
}
