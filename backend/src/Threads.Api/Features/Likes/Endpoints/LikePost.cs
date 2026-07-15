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

    private static async Task<Results<NoContent, NotFound>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = claimsPrincipal.GetUserId();

        var post = await db
            .Posts.Where(post => post.Id == request.PostId)
            .WhereVisibleTo(currentUserId, db)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return TypedResults.NotFound();

        bool likeExists = await db.PostLikes.AnyAsync(
            like => like.PostId == post.Id && like.UserId == currentUserId,
            cancellationToken
        );

        if (likeExists)
            return TypedResults.NoContent();

        var postLike = new PostLike
        {
            PostId = post.Id,
            UserId = currentUserId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await db.PostLikes.AddAsync(postLike, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
