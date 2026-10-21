using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Posts.Endpoints;

public class GetUserPosts : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/{userId}/posts", Handle)
            .WithValidation<Request>()
            .WithSummary("Retrieves posts by userId");
    }

    public record Request(Guid UserId, string? Cursor, int PageSize = PaginationDefaults.PageSize)
        : PagedRequest(Cursor, PageSize);

    public class GetUserPostsValidator : AbstractValidator<Request>
    {
        public GetUserPostsValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    private static async Task<Ok<PagedResponse<PostDto>>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var loggedInUserId = claimsPrincipal.TryGetUserId();

        var posts = await db
            .Posts.Where(post => post.UserId == request.UserId)
            .WhereVisibleTo(loggedInUserId, db)
            .ToDto(loggedInUserId, db)
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(posts);
    }
}
