using Threads.Api.Data.Follows;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared;
using Threads.Api.Features.Blocks;

namespace Threads.Api.Features.Posts;

public static class PostVisibilityExtensions
{
    // visibility rules
    //  # requester can access:
    //      - public posts
    //      - own posts
    //      - followed private posts
    //  # requester can't access:
    //      - blocked user-s posts
    //      - posts from users who blocked the requester
    public static IQueryable<Post> WhereVisibleTo(
        this IQueryable<Post> posts,
        Guid currentUserId,
        AppDbContext db
    )
    {
        return posts
            .WhereOwnerHasNoBlockRelationshipWith(currentUserId, db)
            .Where(post =>
                !post.User.IsPrivate
                || post.UserId == currentUserId
                || db.Follows.Any(follow =>
                    follow.FollowerId == currentUserId
                    && follow.FollowedId == post.UserId
                    && follow.Status == FollowStatus.Accepted
                )
            );
    }
}
