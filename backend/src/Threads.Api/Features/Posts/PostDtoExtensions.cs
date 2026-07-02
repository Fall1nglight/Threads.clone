using Threads.Api.Data.Posts;
using Threads.Api.Data.Users;

namespace Threads.Api.Features.Posts;

public static class PostDtoExtensions
{
    public static IQueryable<PostDto> ToDto(this IQueryable<Post> query)
    {
        return query.Select(post => new PostDto
        {
            Id = post.Id,
            User = new UserDto { Id = post.User.Id, Username = post.User.UserName! },
            Content = post.Content,
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
            CreatedAtUtc = post.CreatedAtUtc,
            UpdatedAtUtc = post.UpdatedAtUtc,
        };
    }

    public static UserDto ToDto(this User user)
    {
        return new UserDto { Id = user.Id, Username = user.UserName! };
    }
}
