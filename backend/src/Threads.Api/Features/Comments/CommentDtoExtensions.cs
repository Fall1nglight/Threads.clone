using Threads.Api.Data.Comments;

namespace Threads.Api.Features.Comments;

public static class CommentDtoExtensions
{
    public static IQueryable<CommentDto> ToCommentDto(this IQueryable<Comment> comments)
    {
        return comments.Select(comment => new CommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            User = new CommentUserDto { Id = comment.User.Id, Username = comment.User.UserName! },
            Content = comment.Content,
            CreatedAtUtc = comment.CreatedAtUtc,
            UpdatedAtUtc = comment.UpdatedAtUtc,
        });
    }
}
