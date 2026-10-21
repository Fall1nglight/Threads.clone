using Threads.Api.Features.Notifications;
using Threads.Api.Features.Notifications.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Notifications;

public static class NotificationRequestFactory
{
    public static GetNotifications.Request CreateGetNotificationsRequest(
        NotificationCategory? category = null,
        NotificationStatus? status = null
    )
    {
        return new(category, status, Cursor: null);
    }

    public static MarkNotificationAsSeen.Request CreateMarkNotificationAsSeenRequest(
        Guid? notificationId = null
    )
    {
        return new(notificationId ?? ValidatorTestData.ValidPrimaryGuid);
    }
}
