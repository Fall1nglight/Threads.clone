using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Notifications;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Follows.Endpoints.Outgoing;

public class CancelOutgoingFollowRequest : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost("/follow-requests/outgoing/{followedId}/cancel", Handle)
            .WithValidation<Request>();
    }

    public record Request(Guid FollowedId);

    public class CancelOutgoingFollowRequestValidator : AbstractValidator<Request>
    {
        public CancelOutgoingFollowRequestValidator()
        {
            RuleFor(x => x.FollowedId).NotEmpty();
        }
    }

    private static async Task<NoContent> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var loggedInUserId = claimsPrincipal.GetUserId();

        var follow = await db.Follows.FirstOrDefaultAsync(
            f =>
                f.FollowerId == loggedInUserId
                && f.FollowedId == request.FollowedId
                && f.Status == FollowStatus.Pending,
            cancellationToken
        );

        if (follow == null)
            return TypedResults.NoContent();

        var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        await db
            .Notifications.Where(notification =>
                notification.TriggeredByUserId == loggedInUserId
                && notification.NotifiedUserId == request.FollowedId
                && notification.Type == NotificationType.FollowRequestReceived
            )
            .ExecuteDeleteAsync(cancellationToken);

        db.Follows.Remove(follow);

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
