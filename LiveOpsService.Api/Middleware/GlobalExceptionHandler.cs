using LiveOpsService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Diagnostics;

namespace LiveOpsService.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception is IAppException appException
            ? appException.StatusCode
            : StatusCodes.Status500InternalServerError;

        var message = exception is IAppException
            ? exception.Message
            : "Internal server error";

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);

        return true;
    }
}
