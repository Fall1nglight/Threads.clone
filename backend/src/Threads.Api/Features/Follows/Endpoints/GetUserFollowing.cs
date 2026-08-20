using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Follows.Endpoints;

public class GetUserFollowing : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{userId}/following", Handle).WithSummary("Retrieves followed users");
    }

    public record Request(Guid UserId, string? Cursor, int PageSize = 20);

    private static async Task<Ok<PagedResponse<FollowUserDto>>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = claimsPrincipal.GetUserId();
        var pagedRequest = new PagedRequest(request.Cursor, request.PageSize);

        var users = await db
            .Follows.Where(follow =>
                follow.FollowerId == request.UserId && follow.Status == FollowStatus.Accepted
            )
            .WhereParticipantsHaveNoBlockRelationship(db)
            .ToExtendedFollowedDto()
            .ToPagedResponse(pagedRequest, cancellationToken);

        return TypedResults.Ok(users);
    }
}
