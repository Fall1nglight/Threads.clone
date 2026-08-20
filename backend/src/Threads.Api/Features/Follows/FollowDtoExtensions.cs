using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;

namespace Threads.Api.Features.Follows;

public static class FollowDtoExtensions
{
    public static IQueryable<FollowUserDto> ToExtendedFollowerDto(
        this IQueryable<Follow> follows,
        Guid? currentUserId,
        AppDbContext db
    )
    {
        return follows.Select(follow => new FollowUserDto
        {
            Id = follow.Follower.Id,
            Username = follow.Follower.UserName!,
            IsPrivate = follow.Follower.IsPrivate,
            CreatedAtUtc = follow.CreatedAtUtc,
            FollowStatusWithCurrentUser = currentUserId.HasValue
                ? db
                    .Follows.Where(f =>
                        f.FollowerId == currentUserId && f.FollowedId == f.FollowerId
                    )
                    .Select(f => (FollowStatus?)f.Status)
                    .FirstOrDefault()
                : null,
        });
    }

    public static IQueryable<FollowUserDto> ToExtendedFollowedDto(this IQueryable<Follow> follows)
    {
        return follows.Select(follow => new FollowUserDto
        {
            Id = follow.Followed.Id,
            Username = follow.Followed.UserName!,
            IsPrivate = follow.Followed.IsPrivate,
            CreatedAtUtc = follow.CreatedAtUtc,
            FollowStatusWithCurrentUser = FollowStatus.Accepted,
        });
    }

    public static IQueryable<FollowUserDto> ToFollowerDto(this IQueryable<Follow> follows)
    {
        return follows.Select(follow => new FollowUserDto
        {
            Id = follow.Follower.Id,
            Username = follow.Follower.UserName!,
            IsPrivate = follow.Follower.IsPrivate,
            CreatedAtUtc = follow.CreatedAtUtc,
        });
    }

    public static IQueryable<FollowUserDto> ToFollowedDto(this IQueryable<Follow> follows)
    {
        return follows.Select(follow => new FollowUserDto
        {
            Id = follow.Followed.Id,
            Username = follow.Followed.UserName!,
            IsPrivate = follow.Followed.IsPrivate,
            CreatedAtUtc = follow.CreatedAtUtc,
        });
    }
}
