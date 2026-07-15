using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;

namespace Threads.Api.Features.Users;

public static class UserVisibilityExtensions
{
    // visibility rules:
    //  - current user can't see blocked users
    //  - current user can't see users who blocked them
    public static IQueryable<User> WhereVisibleTo(
        this IQueryable<User> users,
        Guid currentUserId,
        AppDbContext db
    )
    {
        return users.Where(user =>
            !db.UserBlocks.Any(block =>
                block.BlockerId == currentUserId && block.BlockedId == user.Id
                || block.BlockerId == user.Id && block.BlockedId == currentUserId
            )
        );
    }
}
