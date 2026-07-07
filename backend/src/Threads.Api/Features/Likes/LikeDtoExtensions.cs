using Threads.Api.Data.Likes;

namespace Threads.Api.Features.Likes;

public static class LikeDtoExtensions
{
    public static IQueryable<PostLikeUserDto> ToPostLikeUserDto(this IQueryable<PostLike> query)
    {
        return query.Select(like => new PostLikeUserDto
        {
            Id = like.User.Id,
            Username = like.User.UserName!,
            IsPrivate = like.User.IsPrivate,
            CreatedAtUtc = like.CreatedAtUtc,
        });
    }
}
