using Threads.Api.Data.Follows;
using Threads.Api.Data.Shared;

namespace Threads.Api.Features.Follows;

public static class FollowVisibilityExtensions
{
    public static IQueryable<Follow> WhereBetween(
        this IQueryable<Follow> follows,
        Guid firstUserId,
        Guid secondUserId
    )
    {
        return follows.Where(follow =>
            (follow.FollowerId == firstUserId && follow.FollowedId == secondUserId)
            || (follow.FollowerId == secondUserId && follow.FollowedId == firstUserId)
        );
    }

    public static IQueryable<Follow> WhereParticipantsHaveNoBlockRelationship(
        this IQueryable<Follow> follows,
        AppDbContext db
    )
    {
        return follows.Where(follow =>
            !db.UserBlocks.Any(block =>
                (block.BlockerId == follow.FollowerId && block.BlockedId == follow.FollowedId)
                || (block.BlockerId == follow.FollowedId && block.BlockedId == follow.FollowerId)
            )
        );
    }
}
