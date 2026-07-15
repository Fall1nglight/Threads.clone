using Threads.Api.Data.Comments;
using Threads.Api.Data.Shared;
using Threads.Api.Features.Blocks;
using Threads.Api.Features.Posts;

namespace Threads.Api.Features.Comments;

public static class CommentVisibilityExtensions
{
    public static IQueryable<Comment> WhereVisibleTo(
        this IQueryable<Comment> comments,
        Guid currentUserId,
        AppDbContext db
    )
    {
        var visiblePosts = db.Posts.WhereVisibleTo(currentUserId, db);

        return comments
            .WhereOwnerHasNoBlockRelationshipWith(currentUserId, db)
            .Where(comment => visiblePosts.Any(post => post.Id == comment.PostId));
    }
}
