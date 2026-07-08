using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Data.Users;

namespace Threads.Api.Data.Comments;

public class Comment : IEntity, IOwnedEntity
{
    public Guid Id { get; init; }
    public required Guid PostId { get; init; }
    public Post Post { get; init; } = null!;
    public required Guid UserId { get; init; }
    public User User { get; init; } = null!;
    public required string Content { get; set; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }
}
