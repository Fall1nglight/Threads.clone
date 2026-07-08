using Threads.Api.Common.Pagination;

namespace Threads.Api.Features.Users;

public record UserProfileDto : ICursorItem
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required bool IsPrivate { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public string? Bio { get; init; }
}
