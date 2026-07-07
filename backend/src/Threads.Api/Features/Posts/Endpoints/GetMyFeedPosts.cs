using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Posts.Endpoints;

public class GetMyFeedPosts : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/my-feed", Handle)
            .WithSummary("Retrieves posts from followed accounts and the authenticated user");
    }

    private static async Task<Ok<PagedResponse<PostDto>>> Handle(
        [AsParameters] PagedRequest request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var posts = await db
            .Posts.WhereVisibleInMyFeed(db, userId)
            .ToDto(db, userId)
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(posts);
    }
}
