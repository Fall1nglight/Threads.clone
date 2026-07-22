using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Blocks;

public static class BlockVisibilityExtensions
{
    public static IQueryable<TEntity> WhereOwnerHasNoBlockRelationshipWith<TEntity>(
        this IQueryable<TEntity> query,
        Guid currentUserId,
        AppDbContext db
    )
        where TEntity : class, IOwnedEntity
    {
        return query.Where(entity =>
            !db.UserBlocks.Any(block =>
                (block.BlockerId == currentUserId && block.BlockedId == entity.UserId)
                || (block.BlockerId == entity.UserId && block.BlockedId == currentUserId)
            )
        );
    }
}
