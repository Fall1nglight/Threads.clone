using Threads.Api.Data.Posts;
using Threads.Api.Data.Users;

namespace Threads.Api.Data.Likes;

public class PostLike
{
    public Guid PostId { get; init; }
    public Post Post { get; init; } = null!;
    public Guid UserId { get; init; }
    public User User { get; init; } = null!;
    public required DateTime CreatedAtUtc { get; init; }
}
