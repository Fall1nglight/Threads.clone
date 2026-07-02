namespace Threads.Api.Common.Pagination;

public record Cursor(DateTime LastDate, Guid LastId);

public record PagedRequest
{
    public string? Cursor { get; init; }
    public int PageSize { get; init; }

    public PagedRequest(string? cursor, int pageSize = 20)
    {
        Cursor = cursor;
        PageSize = int.Clamp(pageSize, 1, 100);
    }
}

public record PagedResponse<TResponseDto>(List<TResponseDto> Items, string? Cursor, bool HasMore);
