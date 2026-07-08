using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Users.Endpoints;

public class GetUsers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("", Handle).WithSummary("Retrieves user profiles");
    }

    private static async Task<Ok<PagedResponse<UserProfileDto>>> Handle(
        [AsParameters] PagedRequest request,
        AppDbContext db,
        CancellationToken cancellationToken
    )
    {
        var users = await db.Users.ToUserProfileDto().ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(users);
    }
}
