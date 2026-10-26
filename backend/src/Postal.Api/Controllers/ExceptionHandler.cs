using Microsoft.AspNetCore.Diagnostics;
using Postal.Api.Domain;

namespace Postal.Api.Controllers;

/// <summary>Turns business-rule exceptions into ProblemDetails responses.</summary>
public class ExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            DomainException => (StatusCodes.Status409Conflict, "Conflict"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (0, string.Empty)
        };
        if (status == 0)
            return false; // unexpected error: let the default 500 handling run

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Status = status, Title = title, Detail = exception.Message }
        });
    }
}
