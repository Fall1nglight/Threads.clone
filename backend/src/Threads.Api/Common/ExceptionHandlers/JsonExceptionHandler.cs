using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Threads.Api.Common.ExceptionHandlers;

public class JsonExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<JsonExceptionHandler> _logger;
    private const string ErrorIndicatorText = "could not be mapped to any .NET member";

    public JsonExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<JsonExceptionHandler> logger
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
        if (exception is not BadHttpRequestException badRequestException)
            return false;

        if (badRequestException.InnerException is not JsonException jsonException)
            return false;

        var requestPath = httpContext.Request.GetEncodedPathAndQuery();

        // note: if other exceptions need handling consider using switch and abstraction
        //          but for now it is okay
        return jsonException.Message.Contains(ErrorIndicatorText)
            ? await HandleAdditionalProperties(httpContext, jsonException, requestPath)
            : await HandleMalformedBody(httpContext, jsonException, requestPath);
    }

    private async ValueTask<bool> HandleAdditionalProperties(
        HttpContext httpContext,
        JsonException jsonException,
        string requestPath
    )
    {
        _logger.LogWarning(
            "Invalid request DTO at {RequestPath}. Reason: Additional properties are not allowed.",
            requestPath
        );

        const int statusCode = StatusCodes.Status400BadRequest;
        var problemDetails = new ProblemDetails()
        {
            Title = "Validation error",
            Detail = "Request body is not allowed to contain additional properties.",
            Status = statusCode,
        };

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext()
            {
                HttpContext = httpContext,
                Exception = jsonException,
                ProblemDetails = problemDetails,
            }
        );
    }

    private async ValueTask<bool> HandleMalformedBody(
        HttpContext httpContext,
        JsonException jsonException,
        string requestPath
    )
    {
        _logger.LogWarning(
            "Malformed JSON request received, at: {RequestPath}. Reason: {Reason}",
            requestPath,
            jsonException.Message
        );

        const int statusCode = StatusCodes.Status400BadRequest;
        var problemDetails = new ProblemDetails()
        {
            Title = "Malformed request",
            Detail = "The request body contains invalid JSON syntax.",
            Status = statusCode,
        };

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext()
            {
                HttpContext = httpContext,
                Exception = jsonException,
                ProblemDetails = problemDetails,
            }
        );
    }
}
