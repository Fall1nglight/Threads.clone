using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Likes;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Posts;

namespace Threads.Api.Features.Likes.Endpoints;

public class LikePost : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost("/{postId}/likes", Handle)
            .WithValidation<Request>()
            .WithSummary("Likes a post");
    }

    public record Request(Guid PostId);

    public class LikePostValidator : AbstractValidator<Request>
    {
        public LikePostValidator()
        {
            RuleFor(x => x.PostId).NotEmpty();
        }
    }

    private static async Task<Results<NoContent, NotFound, ForbidHttpResult>> Handle(
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

        bool likeExists = await db.PostLikes.AnyAsync(
            like => like.PostId == post.Id && like.UserId == userId,
            cancellationToken
        );

        if (likeExists)
            return TypedResults.NoContent();

        var postLike = new PostLike
        {
            PostId = post.Id,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await db.PostLikes.AddAsync(postLike, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
