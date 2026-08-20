using Threads.Api.Common.Pagination;
using Threads.Api.Data.Follows;

namespace Threads.Api.Features.Follows;

public record FollowStatusResponse(FollowStatus Status);

public record FollowUserDto : ICursorItem
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required bool IsPrivate { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public FollowStatus? FollowStatusWithCurrentUser { get; init; }
}
