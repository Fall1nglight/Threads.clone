using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Blocks;

namespace Threads.Api.Features.Posts.Endpoints;

public class GetMyFeedPosts : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/personal-feed", Handle)
            .WithSummary("Retrieves posts from followed accounts and the authenticated user");
    }

    private static async Task<Ok<PagedResponse<PostDto>>> Handle(
        [AsParameters] PagedRequest request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = claimsPrincipal.GetUserId();
        var posts = await db
            .Posts.Where(post =>
                post.UserId == currentUserId
                || db.Follows.Any(follow =>
                    follow.FollowerId == currentUserId
                    && follow.FollowedId == post.UserId
                    && follow.Status == FollowStatus.Accepted
                )
            )
            .WhereOwnerHasNoBlockRelationshipWith(currentUserId, db)
            .ToDto(currentUserId, db)
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(posts);
    }
}
