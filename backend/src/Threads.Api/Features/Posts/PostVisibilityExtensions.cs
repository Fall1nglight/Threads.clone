using Threads.Api.Data.Follows;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared;
using Threads.Api.Features.Blocks;

namespace Threads.Api.Features.Posts;

public static class PostVisibilityExtensions
{
    public static IQueryable<Post> WhereVisibleTo(
        this IQueryable<Post> posts,
        Guid? loggedInUserId,
        AppDbContext db
    )
    {
        return loggedInUserId.HasValue
            ? posts.WhereVisibleTo(loggedInUserId.Value, db)
            : posts.Where(post => !post.User.IsPrivate);
    }

    private static IQueryable<Post> WhereVisibleTo(
        this IQueryable<Post> posts,
        Guid loggedInUserId,
        AppDbContext db
    )
    {
        return posts
            .WhereOwnerHasNoBlockRelationshipWith(loggedInUserId, db)
            .Where(post =>
                !post.User.IsPrivate
                || post.UserId == loggedInUserId
                || db.Follows.Any(follow =>
                    follow.FollowerId == loggedInUserId
                    && follow.FollowedId == post.UserId
                    && follow.Status == FollowStatus.Accepted
                )
            );
    }
}
