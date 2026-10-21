namespace Threads.Api.Common.Pagination;

public interface ICursorItem
{
    DateTime CreatedAtUtc { get; }
    Guid Id { get; }
}
