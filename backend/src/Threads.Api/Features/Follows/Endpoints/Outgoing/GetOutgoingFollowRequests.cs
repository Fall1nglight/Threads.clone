using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Follows.Endpoints.Outgoing;

public class GetOutgoingFollowRequests : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/follow-requests/outgoing", Handle)
            .WithSummary("Retrieves outgoing pending follow requests");
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
                follow.FollowerId == loggedInUserId && follow.Status == FollowStatus.Pending
            )
            .WhereParticipantsHaveNoBlockRelationship(db)
            .ToFollowedDto(loggedInUserId, db)
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(users);
    }
}
