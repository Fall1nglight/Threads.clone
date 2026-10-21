using Microsoft.EntityFrameworkCore;

namespace Threads.Api.Common.Pagination;

public static class PagedQueryableExtensions
{
    public static async Task<PagedResponse<TResponseDto>> ToPagedResponse<TResponseDto>(
        this IQueryable<TResponseDto> query,
        PagedRequest request,
        CancellationToken cancellationToken
    )
        where TResponseDto : class, ICursorItem
    {
        if (!string.IsNullOrEmpty(request.Cursor))
        {
            Cursor? decodedCursor = CursorEncoder.Decode(request.Cursor);

            if (decodedCursor != null)
            {
                query = query.Where(x =>
                    x.CreatedAtUtc < decodedCursor.LastDate
                    || (x.CreatedAtUtc == decodedCursor.LastDate && x.Id < decodedCursor.LastId)
                );
            }
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(request.PageSize + 1)
            .ToListAsync(cancellationToken);

        string? encodedCursor = null;
        bool hasMore = items.Count > request.PageSize;

        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);

            TResponseDto lastItem = items.Last();
            encodedCursor = CursorEncoder.Encode(lastItem.CreatedAtUtc, lastItem.Id);
        }

        var response = new PagedResponse<TResponseDto>(items, encodedCursor, hasMore);
        return response;
    }
}
