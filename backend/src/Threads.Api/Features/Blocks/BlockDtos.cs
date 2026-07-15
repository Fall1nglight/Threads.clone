using Threads.Api.Common.Pagination;

namespace Threads.Api.Features.Blocks;

public record BlockedUserDto : ICursorItem
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required bool IsPrivate { get; init; }
    public string? Bio { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
