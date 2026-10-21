using Microsoft.AspNetCore.Http.HttpResults;

namespace Threads.Api.Common;

public static class HttpResultFactory
{
    public static ProblemHttpResult Unauthorized(string? detail)
    {
        return TypedResults.Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            detail: detail ?? string.Empty
        );
    }
}
