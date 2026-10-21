using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Follows.Endpoints.Incoming;

public class GetIncomingFollowRequests : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/follow-requests/incoming", Handle)
            .WithSummary("Retrieves incoming pending follow requests");
    }

    private static async Task<Ok<PagedResponse<FollowUserDto>>> Handle(
        [AsParameters] PagedRequest request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var loggedInUserId = claimsPrincipal.GetUserId();
        var users = await db
            .Follows.Where(follow =>
                follow.FollowedId == loggedInUserId && follow.Status == FollowStatus.Pending
            )
            .WhereParticipantsHaveNoBlockRelationship(db)
            .ToFollowerDto(loggedInUserId, db)
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(users);
    }
}
