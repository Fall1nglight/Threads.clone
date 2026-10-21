using Threads.Api.Data.Notifications;

namespace Threads.Api.Features.Notifications;

public static class NotificationDtoExtensions
{
    public static IQueryable<NotificationDto> ToDto(this IQueryable<Notification> notifications)
    {
        return notifications.Select(notification => new NotificationDto
        {
            Id = notification.Id,
            TriggeredByUser = new NotificationUserDto
            {
                Id = notification.TriggeredByUser.Id,
                Username = notification.TriggeredByUser.UserName!,
            },
            Type = notification.Type,
            SubjectKind = notification.SubjectKind,
            SubjectId = notification.SubjectId,
            SeenAtUtc = notification.SeenAtUtc,
            CreatedAtUtc = notification.TriggeredAtUtc,
        });
    }
}
