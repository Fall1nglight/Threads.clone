using Threads.Api.Common.Pagination;

namespace Threads.Api.Features.Comments;

public record CommentDto : ICursorItem
{
    public required Guid Id { get; init; }
    public required Guid PostId { get; init; }
    public required CommentUserDto User { get; init; }
    public required string Content { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

public record CommentUserDto
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
}
