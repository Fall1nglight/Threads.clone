using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Comments.Endpoints;

public class UpdateComment : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPut("/{postId}/comments/{commentId}", Handle)
            .WithValidation<Request>()
            .WithSummary("Updates a comment");
    }

    public record Request(Guid PostId, Guid CommentId, [FromBody] Body Body);

    public record Body(string Content);

    public class UpdateCommentValidator : AbstractValidator<Request>
    {
        public UpdateCommentValidator()
        {
            RuleFor(x => x.PostId).NotEmpty();
            RuleFor(x => x.CommentId).NotEmpty();
            RuleFor(x => x.Body.Content).NotEmpty().MaximumLength(600);
        }
    }

    private static async Task<Results<NoContent, NotFound, ForbidHttpResult>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var comment = await db.Comments.FirstOrDefaultAsync(
            c => c.PostId == request.PostId && c.Id == request.CommentId,
            cancellationToken
        );

        if (comment == null)
            return TypedResults.NotFound();

        var userId = claimsPrincipal.GetUserId();
        if (comment.UserId != userId)
            return TypedResults.Forbid();

        comment.Content = request.Body.Content;
        comment.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
