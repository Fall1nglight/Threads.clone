using Threads.Api.Common.Pagination;

namespace Threads.Api.Features.Posts;

public record PostDto : ICursorItem
{
    public required Guid Id { get; init; }
    public required UserDto User { get; init; }
    public required string Content { get; init; }
    public required int LikeCount { get; init; }
    public required bool IsLikedByCurrentUser { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
};

public record UserDto
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
};
