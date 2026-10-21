using Threads.Api.Features.Comments.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Comments;

public static class CommentRequestFactory
{
    // content does not default to null, as we would like to validate agains null values
    public static CreateComment.Request CreateCreateCommentRequest(
        Guid? postId = null,
        string? content = ValidatorTestData.ValidContent
    )
    {
        return new(postId ?? ValidatorTestData.ValidPrimaryGuid, new CreateComment.Body(content!));
    }

    // content does not default to null, as we would like to validate agains null values
    public static UpdateComment.Request CreateUpdateCommentRequest(
        Guid? postId = null,
        Guid? commentId = null,
        string? content = ValidatorTestData.ValidContent
    )
    {
        return new(
            postId ?? ValidatorTestData.ValidPrimaryGuid,
            commentId ?? ValidatorTestData.ValidSecondaryGuid,
            new UpdateComment.Body(content!)
        );
    }

    public static DeleteComment.Request CreateDeleteCommentRequest(
        Guid? postId = null,
        Guid? commentId = null
    )
    {
        return new(
            postId ?? ValidatorTestData.ValidPrimaryGuid,
            commentId ?? ValidatorTestData.ValidSecondaryGuid
        );
    }

    public static GetComments.Request CreateGetCommentsRequest(Guid? postId = null)
    {
        return new(postId ?? ValidatorTestData.ValidPrimaryGuid, Cursor: null);
    }
}
