using Threads.Api.Data.Follows;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared;

namespace Threads.Api.Features.Posts;

public static class PostVisibilityExtensions
{
    public static IQueryable<Post> WhereVisibleInAnonymousFeed(this IQueryable<Post> posts)
    {
        return posts.Where(p => !p.User.IsPrivate);
    }

    public static IQueryable<Post> WhereVisibleInGlobalFeed(
        this IQueryable<Post> posts,
        AppDbContext db,
        Guid userId
    )
    {
        return posts.Where(p =>
            // public users' posts
            !p.User.IsPrivate
            // requester's posts
            || p.UserId == userId
            // posts from accounts which are followed by the requester
            || db.Follows.Any(follow =>
                follow.FollowerId == userId
                && follow.FollowedId == p.UserId
                && follow.Status == FollowStatus.Accepted
            )
        );
    }

    public static IQueryable<Post> WhereVisibleInMyFeed(
        this IQueryable<Post> posts,
        AppDbContext db,
        Guid userId
    )
    {
        return posts.Where(p =>
            p.UserId == userId
            || db.Follows.Any(f =>
                f.FollowerId == userId
                && f.FollowedId == p.UserId
                && f.Status == FollowStatus.Accepted
            )
        );
    }
}
