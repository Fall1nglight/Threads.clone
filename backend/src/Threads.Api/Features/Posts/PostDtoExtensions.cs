using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;

namespace Threads.Api.Features.Posts;

public static class PostDtoExtensions
{
    public static IQueryable<PostDto> ToDto(this IQueryable<Post> query, AppDbContext db)
    {
        return query.ToDtoCore(db, null);
    }

    public static IQueryable<PostDto> ToDto(
        this IQueryable<Post> query,
        AppDbContext db,
        Guid userId
    )
    {
        return query.ToDtoCore(db, userId);
    }

    private static IQueryable<PostDto> ToDtoCore(
        this IQueryable<Post> query,
        AppDbContext db,
        Guid? userId
    )
    {
        return query.Select(post => new PostDto
        {
            Id = post.Id,
            User = new UserDto { Id = post.User.Id, Username = post.User.UserName! },
            Content = post.Content,
            LikeCount = db.PostLikes.Count(like => like.PostId == post.Id),
            IsLikedByCurrentUser =
                userId.HasValue
                && db.PostLikes.Any(like => like.PostId == post.Id && like.UserId == userId.Value),
            CreatedAtUtc = post.CreatedAtUtc,
            UpdatedAtUtc = post.UpdatedAtUtc,
        });
    }

    public static PostDto ToDto(this Post post) => post.ToDto(post.User);

    public static PostDto ToDto(this Post post, User user)
    {
        return new PostDto
        {
            Id = post.Id,
            User = user.ToDto(),
            Content = post.Content,
            LikeCount = 0,
            IsLikedByCurrentUser = false,
            CreatedAtUtc = post.CreatedAtUtc,
            UpdatedAtUtc = post.UpdatedAtUtc,
        };
    }

    public static UserDto ToDto(this User user)
    {
        return new UserDto { Id = user.Id, Username = user.UserName! };
    }
}
