using Threads.Api.Data.Users;

namespace Threads.Api.Data.Follows;

public class Follow
{
    public Guid FollowerId { get; init; }
    public User Follower { get; set; } = null!;
    public Guid FollowedId { get; init; }
    public User Followed { get; set; } = null!;
    public FollowStatus Status { get; set; } = FollowStatus.Pending;
    public DateTime CreatedAtUtc { get; init; }
}
