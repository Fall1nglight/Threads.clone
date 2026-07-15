using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Posts.Endpoints;

public class GetGlobalFeedPosts : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/global-feed", Handle)
            .WithSummary("Retrieves global feed posts visible to the authenticated user");
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
            .Posts.WhereVisibleTo(currentUserId, db)
            .ToDto(currentUserId, db)
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(posts);
    }
}
