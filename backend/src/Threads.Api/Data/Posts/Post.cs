using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Data.Users;

namespace Threads.Api.Data.Posts;

public class Post : IEntity, IOwnedEntity
{
    public Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public User User { get; init; } = null!;
    public required string Content { get; set; }
    public bool IsDeleted { get; set; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }
}
