using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Data.Users;

namespace Threads.Api.Data.Notifications;

public class Notification : IEntity
{
    public Guid Id { get; init; }
    public required Guid TriggeredByUserId { get; init; }
    public User TriggeredByUser { get; init; } = null!;
    public required DateTime TriggeredAtUtc { get; init; }
    public required Guid NotifiedUserId { get; init; }
    public User NotifiedUser { get; init; } = null!;
    public required NotificationType Type { get; set; }
    public NotificationSubjectKind? SubjectKind { get; init; }
    public Guid? SubjectId { get; init; }
    public DateTime? SeenAtUtc { get; set; }
}
