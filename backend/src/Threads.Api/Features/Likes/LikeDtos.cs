using Threads.Api.Common.Pagination;

namespace Threads.Api.Features.Likes;

public record PostLikeUserDto : ICursorItem
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required bool IsPrivate { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
