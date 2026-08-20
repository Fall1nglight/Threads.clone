using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Posts.Endpoints;

public class GetUserPosts : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("", Handle).WithSummary("Retrieves posts by userId");
    }

    // userId comes from query parameters so we dont specify it in the route path
    public record Request(Guid UserId, string? Cursor, int PageSize = 20);

    private static async Task<Ok<PagedResponse<PostDto>>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = claimsPrincipal.GetUserId();
        var pagedRequest = new PagedRequest(request.Cursor, request.PageSize);
        var posts = await db
            .Posts.Where(post => post.UserId == request.UserId)
            .WhereVisibleTo(currentUserId, db)
            .ToDto(currentUserId, db)
            .ToPagedResponse(pagedRequest, cancellationToken);
        return TypedResults.Ok(posts);
    }
}
