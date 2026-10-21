using Threads.Api.Data.Comments;
using Threads.Api.Data.Shared;
using Threads.Api.Features.Blocks;
using Threads.Api.Features.Posts;

namespace Threads.Api.Features.Comments;

public static class CommentVisibilityExtensions
{
    public static IQueryable<Comment> WhereVisibleTo(
        this IQueryable<Comment> comments,
        Guid loggedInUserId,
        AppDbContext db
    )
    {
        var visiblePosts = db.Posts.WhereVisibleTo(loggedInUserId, db);

        return comments
            .WhereOwnerHasNoBlockRelationshipWith(loggedInUserId, db)
            .Where(comment => visiblePosts.Any(post => post.Id == comment.PostId));
    }
}
