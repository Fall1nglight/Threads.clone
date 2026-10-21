using Threads.Api.Common.Pagination;
using Threads.Api.Data.Notifications;

namespace Threads.Api.Features.Notifications;

public record NotificationDto : ICursorItem
{
    public required Guid Id { get; init; }
    public required NotificationUserDto TriggeredByUser { get; init; }
    public required NotificationType Type { get; init; }
    public NotificationSubjectKind? SubjectKind { get; init; }
    public Guid? SubjectId { get; init; }
    public DateTime? SeenAtUtc { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}

public record NotificationUserDto
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
}

public sealed record UnseenNotificationCountDto(int Count);
