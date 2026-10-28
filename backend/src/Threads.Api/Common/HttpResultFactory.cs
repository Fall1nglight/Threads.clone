using Microsoft.AspNetCore.Http.HttpResults;

namespace Threads.Api.Common;

public static class HttpResultFactory
{
    public static ProblemHttpResult Create(int statusCode, string? detail)
    {
        return TypedResults.Problem(statusCode: statusCode, detail: detail ?? "Unknown error");
    }
}
