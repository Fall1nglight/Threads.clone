using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared;
using Threads.Api.Features.Blocks;

namespace Threads.Api.Features.Posts;

public static class PostDtoExtensions
{
    public static IQueryable<PostDto> ToDto(this IQueryable<Post> posts, AppDbContext db)
    {
        return posts.Select(post => new PostDto
        {
            Id = post.Id,
            User = new UserDto { Id = post.User.Id, Username = post.User.UserName! },
            Content = post.Content,
            LikeCount = db.PostLikes.Count(like => like.PostId == post.Id),
            CommentCount = db.Comments.Count(comment => comment.PostId == post.Id),
            IsLikedByCurrentUser = false,
            CreatedAtUtc = post.CreatedAtUtc,
            UpdatedAtUtc = post.UpdatedAtUtc,
        });
    }

    public static IQueryable<PostDto> ToDto(
        this IQueryable<Post> posts,
        Guid currentUserId,
        AppDbContext db
    )
    {
        return posts.Select(post => new PostDto
        {
            Id = post.Id,
            User = new UserDto { Id = post.User.Id, Username = post.User.UserName! },
            Content = post.Content,
            LikeCount = db
                .PostLikes.WhereOwnerHasNoBlockRelationshipWith(currentUserId, db)
                .Count(like => like.PostId == post.Id),
            CommentCount = db
                .Comments.WhereOwnerHasNoBlockRelationshipWith(currentUserId, db)
                .Count(comment => comment.PostId == post.Id),
            IsLikedByCurrentUser = db.PostLikes.Any(like =>
                like.PostId == post.Id && like.UserId == currentUserId
            ),
            CreatedAtUtc = post.CreatedAtUtc,
            UpdatedAtUtc = post.UpdatedAtUtc,
        });
    }
}
