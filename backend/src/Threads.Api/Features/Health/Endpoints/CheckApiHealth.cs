using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Health.Endpoints;

public sealed class CheckApiHealth : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/api", Handle)
            .Produces(StatusCodes.Status200OK)
            .WithSummary("Reports whether the API is available");
    }

    private static Ok Handle()
    {
        return TypedResults.Ok();
    }
}
