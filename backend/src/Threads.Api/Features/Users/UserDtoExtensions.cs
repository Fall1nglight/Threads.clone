using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;

namespace Threads.Api.Features.Users;

public static class UserDtoExtensions
{
    public static IQueryable<UserProfileDto> ToUserProfileDto(
        this IQueryable<User> users,
        Guid? currentUserId,
        AppDbContext db
    )
    {
        return users.Select(user => new UserProfileDto
        {
            Id = user.Id,
            Username = user.UserName!,
            IsPrivate = user.IsPrivate,
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc,
            Bio = user.Bio,
            FollowerCount = db.Follows.Count(follow =>
                follow.FollowedId == user.Id && follow.Status == FollowStatus.Accepted
            ),

            FollowingCount = db.Follows.Count(follow =>
                follow.FollowerId == user.Id && follow.Status == FollowStatus.Accepted
            ),

            FollowStatusWithCurrentUser = currentUserId.HasValue
                ? db
                    .Follows.Where(f =>
                        f.FollowerId == currentUserId.Value && f.FollowedId == user.Id
                    )
                    .Select(f => (FollowStatus?)f.Status)
                    .FirstOrDefault()
                : null,
        });
    }
}
