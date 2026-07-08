using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Comments;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Posts;

namespace Threads.Api.Features.Comments.Endpoints;

public class CreateComment : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost("/{postId}/comments", Handle)
            .WithValidation<Request>()
            .WithSummary("Creates a comment on a post");
    }

    public record Request(Guid PostId, [FromBody] Body Body);

    public record Body(string Content);

    public class CreateCommentValidator : AbstractValidator<Request>
    {
        public CreateCommentValidator()
        {
            RuleFor(x => x.PostId).NotEmpty();
            RuleFor(x => x.Body.Content).NotEmpty().MaximumLength(600);
        }
    }

    private static async Task<Results<Created<CommentDto>, NotFound, ForbidHttpResult>> Handle(
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

        var comment = new Comment
        {
            PostId = post.Id,
            UserId = userId,
            Content = request.Body.Content,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await db.Comments.AddAsync(comment, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        CommentDto response = await db
            .Comments.Where(c => c.Id == comment.Id)
            .ToCommentDto()
            .FirstAsync(cancellationToken);

        return TypedResults.Created($"/posts/{post.Id}/comments/{comment.Id}", response);
    }
}
