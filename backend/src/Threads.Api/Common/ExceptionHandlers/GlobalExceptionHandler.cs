using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Threads.Api.Common.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger
    )
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var requestPath = httpContext.Request.GetEncodedPathAndQuery();
        var method = httpContext.Request.Method;

        _logger.LogError(
            exception,
            "Unhandled exception during {Method} request at {RequestPath}.",
            method,
            requestPath
        );

        const int statusCode = StatusCodes.Status500InternalServerError;
        var problemDetails = new ProblemDetails()
        {
            Title = "Internal server error",
            Detail = "An unexpected error occurred while processing your request.",
            Status = statusCode,
        };

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext()
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails,
            }
        );
    }
}
