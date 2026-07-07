using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Follows.Endpoints;

public class GetFollowing : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/following", Handle).WithSummary("Retrieves followed users");
    }

    private static async Task<Ok<PagedResponse<FollowUserDto>>> Handle(
        [AsParameters] PagedRequest request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var followerId = claimsPrincipal.GetUserId();
        var users = await db
            .Follows.Where(follow =>
                follow.FollowerId == followerId && follow.Status == FollowStatus.Accepted
            )
            .ToFollowedDto()
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(users);
    }
}
