using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Posts;

namespace Threads.Api.Features.Comments.Endpoints;

public class GetPostComments : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/{postId}/comments", Handle)
            .WithValidation<Request>()
            .WithSummary("Retrieves comments on a post");
    }

    public record Request(Guid PostId, string? Cursor, int PageSize = 20);

    public class GetPostCommentsValidator : AbstractValidator<Request>
    {
        public GetPostCommentsValidator()
        {
            RuleFor(x => x.PostId).NotEmpty();
        }
    }

    private static async Task<
        Results<Ok<PagedResponse<CommentDto>>, NotFound, ForbidHttpResult>
    > Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var post = await db
            .Posts.Where(p => p.Id == request.PostId)
            .Include(p => p.User)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return TypedResults.NotFound();

        var userId = claimsPrincipal.GetUserId();
        bool canViewPost = await post.CanBeViewedByUserAsync(db, userId, cancellationToken);
        if (!canViewPost)
            return TypedResults.Forbid();

        var pagedRequest = new PagedRequest(request.Cursor, request.PageSize);
        var response = await db
            .Comments.Where(comment => comment.PostId == post.Id)
            .ToCommentDto()
            .ToPagedResponse(pagedRequest, cancellationToken);

        return TypedResults.Ok(response);
    }
}
