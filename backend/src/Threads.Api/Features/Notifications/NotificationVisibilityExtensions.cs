using Threads.Api.Data.Notifications;
using Threads.Api.Data.Shared;

namespace Threads.Api.Features.Notifications;

public static class NotificationVisibilityExtensions
{
    public static IQueryable<Notification> WhereVisibleTo(
        this IQueryable<Notification> notifications,
        Guid loggedInUserId,
        AppDbContext db
    )
    {
        return notifications.Where(notification =>
            notification.NotifiedUserId == loggedInUserId
            && !db.UserBlocks.Any(block =>
                (
                    block.BlockerId == loggedInUserId
                    && block.BlockedId == notification.TriggeredByUserId
                )
                || (
                    block.BlockerId == notification.TriggeredByUserId
                    && block.BlockedId == loggedInUserId
                )
            )
        );
    }

    public static IQueryable<Notification> AnyBetween(
        this IQueryable<Notification> notifications,
        Guid firstUserId,
        Guid secondUserId
    )
    {
        return notifications.Where(x =>
            (x.NotifiedUserId == firstUserId && x.TriggeredByUserId == secondUserId)
            || (x.NotifiedUserId == secondUserId && x.TriggeredByUserId == firstUserId)
        );
    }
}
