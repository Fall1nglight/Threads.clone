using Threads.Api.Data.Follows;

namespace Threads.Api.Features.Follows;

public static class FollowDtoExtensions
{
    public static IQueryable<FollowUserDto> ToFollowerDto(this IQueryable<Follow> query)
    {
        return query.Select(follow => new FollowUserDto
        {
            Id = follow.Follower.Id,
            Username = follow.Follower.UserName!,
            IsPrivate = follow.Follower.IsPrivate,
            CreatedAtUtc = follow.CreatedAtUtc,
        });
    }

    public static IQueryable<FollowUserDto> ToFollowedDto(this IQueryable<Follow> query)
    {
        return query.Select(follow => new FollowUserDto
        {
            Id = follow.Followed.Id,
            Username = follow.Followed.UserName!,
            IsPrivate = follow.Followed.IsPrivate,
            CreatedAtUtc = follow.CreatedAtUtc,
        });
    }
}
