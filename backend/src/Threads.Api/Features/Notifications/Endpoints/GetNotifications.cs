using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Notifications;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Notifications.Endpoints;

public class GetNotifications : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("", Handle)
            .WithValidation<Request>()
            .WithSummary("Retrieves notifications for the current user");
    }

    public record Request(
        NotificationCategory? Category,
        NotificationStatus? Status,
        string? Cursor,
        int PageSize = PaginationDefaults.PageSize
    ) : PagedRequest(Cursor, PageSize);

    public class GetNotificationsValidator : AbstractValidator<Request>
    {
        public GetNotificationsValidator()
        {
            RuleFor(x => x.Category).Must(type => !type.HasValue || Enum.IsDefined(type.Value));

            RuleFor(x => x.Status).Must(status => !status.HasValue || Enum.IsDefined(status.Value));
        }
    }

    private static async Task<Ok<PagedResponse<NotificationDto>>> Handle(
        [AsParameters] Request request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var loggedInUserId = claimsPrincipal.GetUserId();
        var notificationsQuery = db.Notifications.WhereVisibleTo(loggedInUserId, db);

        // todo, move this elsewhere
        if (request.Category.HasValue)
        {
            notificationsQuery = request.Category.Value switch
            {
                NotificationCategory.Follow => notificationsQuery.Where(notification =>
                    notification.Type == NotificationType.Followed
                    || notification.Type == NotificationType.FollowRequestAccepted
                    || notification.Type == NotificationType.FollowRequestReceived
                ),
                NotificationCategory.PostLiked => notificationsQuery.Where(notification =>
                    notification.Type == NotificationType.PostLiked
                ),
                NotificationCategory.PostCommented => notificationsQuery.Where(notification =>
                    notification.Type == NotificationType.PostCommented
                ),
                _ => notificationsQuery,
            };
        }

        if (request.Status.HasValue)
        {
            notificationsQuery =
                request.Status.Value == NotificationStatus.Seen
                    ? notificationsQuery.Where(notification => notification.SeenAtUtc != null)
                    : notificationsQuery.Where(notification => notification.SeenAtUtc == null);
        }

        var notifications = await notificationsQuery
            .ToDto()
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(notifications);
    }
}
