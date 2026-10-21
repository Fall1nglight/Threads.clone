using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;

namespace Threads.Api.Features.Users;

public static class UserVisibilityExtensions
{
    public static IQueryable<User> WhereVisibleTo(
        this IQueryable<User> users,
        Guid? loggedInUserId,
        AppDbContext db
    )
    {
        return loggedInUserId.HasValue ? users.WhereVisibleTo(loggedInUserId.Value, db) : users;
    }

    private static IQueryable<User> WhereVisibleTo(
        this IQueryable<User> users,
        Guid loggedInUserId,
        AppDbContext db
    )
    {
        return users.Where(user =>
            !db.UserBlocks.Any(block =>
                block.BlockerId == loggedInUserId && block.BlockedId == user.Id
                || block.BlockerId == user.Id && block.BlockedId == loggedInUserId
            )
        );
    }
}
