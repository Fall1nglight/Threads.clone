using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Follows.Endpoints;

public class GetFollowers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/followers", Handle).WithSummary("Retrieves followers");
    }

    private static async Task<Ok<PagedResponse<FollowUserDto>>> Handle(
        [AsParameters] PagedRequest request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var followedId = claimsPrincipal.GetUserId();
        var users = await db
            .Follows.Where(follow =>
                follow.FollowedId == followedId && follow.Status == FollowStatus.Accepted
            )
            .ToFollowerDto()
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(users);
    }
}
