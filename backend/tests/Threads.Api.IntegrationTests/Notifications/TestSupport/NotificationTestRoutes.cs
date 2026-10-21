namespace Threads.Api.IntegrationTests.Notifications.TestSupport;

public static class NotificationTestRoutes
{
    public const string Base = "notifications";
    public const string UnseenCount = $"{Base}/unseen-count";

    public static string MarkAsSeen(Guid notificationId) => $"{Base}/{notificationId}/seen";
}
