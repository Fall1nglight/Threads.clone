using Threads.Api.Data.Users;

namespace Threads.Api.Data.Blocks;

public class UserBlock
{
    public Guid BlockerId { get; init; }
    public User Blocker { get; init; } = null!;
    public Guid BlockedId { get; init; }
    public User Blocked { get; init; } = null!;
    public required DateTime CreatedAtUtc { get; init; }
}
