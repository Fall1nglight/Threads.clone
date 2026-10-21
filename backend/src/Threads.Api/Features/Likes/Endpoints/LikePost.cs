using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Likes;
using Threads.Api.Data.Notifications;
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
        var loggedInUserId = claimsPrincipal.GetUserId();

        var post = await db
            .Posts.Where(post => post.Id == request.PostId)
            .WhereVisibleTo(loggedInUserId, db)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return TypedResults.NotFound();

        bool likeExists = await db.PostLikes.AnyAsync(
            like => like.PostId == post.Id && like.UserId == loggedInUserId,
            cancellationToken
        );

        if (likeExists)
            return TypedResults.NoContent();

        var postLike = new PostLike
        {
            PostId = post.Id,
            UserId = loggedInUserId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await db.PostLikes.AddAsync(postLike, cancellationToken);

        if (post.UserId != loggedInUserId)
        {
            var notification = new Notification
            {
                TriggeredByUserId = loggedInUserId,
                NotifiedUserId = post.UserId,
                TriggeredAtUtc = postLike.CreatedAtUtc,
                Type = NotificationType.PostLiked,
                SubjectKind = NotificationSubjectKind.Post,
                SubjectId = post.Id,
            };
            await db.Notifications.AddAsync(notification, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
