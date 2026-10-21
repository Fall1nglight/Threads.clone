using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Notifications.Endpoints;

public class MarkNotificationAsSeen : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost("/{notificationId}/seen", Handle)
            .WithValidation<Request>()
            .WithSummary("Marks a notification as seen");
    }

    public record Request(Guid NotificationId);

    public class MarkNotificationAsSeenValidator : AbstractValidator<Request>
    {
        public MarkNotificationAsSeenValidator()
        {
            RuleFor(x => x.NotificationId).NotEmpty();
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
        var seenAtUtc = DateTime.UtcNow;

        var updatedCount = await db
            .Notifications.Where(notification =>
                notification.Id == request.NotificationId
                && notification.NotifiedUserId == loggedInUserId
                && notification.SeenAtUtc == null
            )
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(notification => notification.SeenAtUtc, seenAtUtc),
                cancellationToken
            );

        if (updatedCount > 0)
            return TypedResults.NoContent();

        var notificationExists = await db.Notifications.AnyAsync(
            notification =>
                notification.Id == request.NotificationId
                && notification.NotifiedUserId == loggedInUserId,
            cancellationToken
        );

        if (!notificationExists)
            return TypedResults.NotFound();

        return TypedResults.NoContent();
    }
}
