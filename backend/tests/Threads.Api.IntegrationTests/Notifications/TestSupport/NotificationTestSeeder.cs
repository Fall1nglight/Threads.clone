using Threads.Api.Data.Notifications;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Notifications.TestSupport;

public sealed class NotificationTestSeeder
{
    private readonly AppDbContext _db;

    public NotificationTestSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Notification> CreateNotificationAsync(
        User triggeredByUser,
        User notifiedUser,
        NotificationType type,
        DateTime? triggeredAtUtc = null,
        NotificationSubjectKind? subjectKind = null,
        Guid? subjectId = null,
        DateTime? seenAtUtc = null
    )
    {
        var notification = new Notification
        {
            TriggeredByUserId = triggeredByUser.Id,
            NotifiedUserId = notifiedUser.Id,
            TriggeredAtUtc = triggeredAtUtc ?? IntegrationTestData.BaseTime,
            Type = type,
            SubjectKind = subjectKind,
            SubjectId = subjectId,
            SeenAtUtc = seenAtUtc,
        };

        await _db.Notifications.AddAsync(notification);
        await _db.SaveChangesAsync();
        return notification;
    }
}
