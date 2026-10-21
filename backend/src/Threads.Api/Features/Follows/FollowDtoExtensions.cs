using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;

namespace Threads.Api.Features.Follows;

public static class FollowDtoExtensions
{
    public static IQueryable<FollowUserDto> ToFollowerDto(
        this IQueryable<Follow> follows,
        Guid? loggedInUserId,
        AppDbContext db
    )
    {
        return follows.Select(follow => new FollowUserDto
        {
            Id = follow.Follower.Id,
            Username = follow.Follower.UserName!,
            IsPrivate = follow.Follower.IsPrivate,
            CreatedAtUtc = follow.CreatedAtUtc,
            FollowStatusWithCurrentUser = loggedInUserId.HasValue
                ? db
                    .Follows.Where(f =>
                        f.FollowerId == loggedInUserId && f.FollowedId == follow.FollowerId
                    )
                    .Select(f => (FollowStatus?)f.Status)
                    .FirstOrDefault()
                : null,
        });
    }

    public static IQueryable<FollowUserDto> ToFollowedDto(
        this IQueryable<Follow> follows,
        Guid? loggedInUserId,
        AppDbContext db
    )
    {
        return follows.Select(follow => new FollowUserDto
        {
            Id = follow.Followed.Id,
            Username = follow.Followed.UserName!,
            IsPrivate = follow.Followed.IsPrivate,
            CreatedAtUtc = follow.CreatedAtUtc,
            FollowStatusWithCurrentUser = loggedInUserId.HasValue
                ? db
                    .Follows.Where(f =>
                        f.FollowerId == loggedInUserId && f.FollowedId == follow.FollowedId
                    )
                    .Select(f => (FollowStatus?)f.Status)
                    .FirstOrDefault()
                : null,
        });
    }
}
