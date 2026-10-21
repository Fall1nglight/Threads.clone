using Threads.Api.Data.Likes;
using Threads.Api.Data.Shared;
using Threads.Api.Features.Blocks;
using Threads.Api.Features.Posts;

namespace Threads.Api.Features.Likes;

public static class LikeVisibilityExtensions
{
    public static IQueryable<PostLike> WhereVisibleTo(
        this IQueryable<PostLike> likes,
        Guid loggedInUserId,
        AppDbContext db
    )
    {
        var visiblePosts = db.Posts.WhereVisibleTo(loggedInUserId, db);

        return likes
            .WhereOwnerHasNoBlockRelationshipWith(loggedInUserId, db)
            .Where(like => visiblePosts.Any(post => post.Id == like.PostId));
    }
}
