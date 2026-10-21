using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Notifications;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Users;

namespace Threads.Api.Features.Follows.Endpoints;

public class SendFollowRequest : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost("/following/{targetUserId}", Handle)
            .WithValidation<Request>()
            .WithSummary("Sends a follow request");
    }

    public record Request(Guid TargetUserId);

    public class SendFollowRequestValidator : AbstractValidator<Request>
    {
        public SendFollowRequestValidator()
        {
            RuleFor(x => x.TargetUserId).NotEmpty();
        }
    }

    private static async Task<Results<Ok<FollowStatusResponse>, NotFound, BadRequest>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var loggedInUserId = claimsPrincipal.GetUserId();
        if (request.TargetUserId == loggedInUserId)
            return TypedResults.BadRequest();

        var targetUser = await db
            .Users.WhereVisibleTo(loggedInUserId, db)
            .FirstOrDefaultAsync(user => user.Id == request.TargetUserId, cancellationToken);

        if (targetUser == null)
            return TypedResults.NotFound();

        var follow = await db.Follows.FirstOrDefaultAsync(
            follow => follow.FollowerId == loggedInUserId && follow.FollowedId == targetUser.Id,
            cancellationToken
        );

        if (follow == null)
        {
            var createdAtUtc = DateTime.UtcNow;
            follow = new Follow
            {
                FollowerId = loggedInUserId,
                FollowedId = targetUser.Id,
                Status = targetUser.IsPrivate ? FollowStatus.Pending : FollowStatus.Accepted,
                CreatedAtUtc = createdAtUtc,
            };

            await db.Follows.AddAsync(follow, cancellationToken);

            var notification = new Notification
            {
                TriggeredByUserId = loggedInUserId,
                NotifiedUserId = targetUser.Id,
                TriggeredAtUtc = createdAtUtc,
                Type = targetUser.IsPrivate
                    ? NotificationType.FollowRequestReceived
                    : NotificationType.Followed,
            };

            await db.Notifications.AddAsync(notification, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        var response = new FollowStatusResponse(follow.Status);
        return TypedResults.Ok(response);
    }
}
