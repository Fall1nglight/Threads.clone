using Threads.Api.Data.Users;

namespace Threads.Api.Features.Users;

public static class UserDtoExtensions
{
    public static IQueryable<UserProfileDto> ToUserProfileDto(this IQueryable<User> users)
    {
        return users.Select(user => new UserProfileDto
        {
            Id = user.Id,
            Username = user.UserName!,
            IsPrivate = user.IsPrivate,
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc,
            Bio = user.Bio,
        });
    }
}
