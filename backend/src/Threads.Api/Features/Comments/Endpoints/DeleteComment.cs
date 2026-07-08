using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Comments.Endpoints;

public class DeleteComment : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapDelete("/{postId}/comments/{commentId}", Handle)
            .WithValidation<Request>()
            .WithSummary("Deletes a comment");
    }

    public record Request(Guid PostId, Guid CommentId);

    public class DeleteCommentValidator : AbstractValidator<Request>
    {
        public DeleteCommentValidator()
        {
            RuleFor(x => x.PostId).NotEmpty();
            RuleFor(x => x.CommentId).NotEmpty();
        }
    }

    private static async Task<Results<NoContent, ForbidHttpResult>> Handle(
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
            return TypedResults.NoContent();

        var userId = claimsPrincipal.GetUserId();
        if (comment.UserId != userId)
            return TypedResults.Forbid();

        db.Comments.Remove(comment);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
