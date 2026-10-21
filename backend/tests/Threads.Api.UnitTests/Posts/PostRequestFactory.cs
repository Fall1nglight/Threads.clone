using Threads.Api.Features.Posts.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Posts;

public static class PostRequestFactory
{
    public static CreatePost.Request CreatePostRequest(
        string? content = ValidatorTestData.ValidContent
    )
    {
        return new(content!);
    }

    public static UpdatePost.Request CreateUpdateRequest(
        Guid? postId = null,
        string? content = ValidatorTestData.ValidContent
    )
    {
        return new(
            postId ?? ValidatorTestData.ValidPrimaryGuid,
            CreateUpdateBody(content: content)
        );
    }

    public static UpdatePost.Body CreateUpdateBody(string? content = ValidatorTestData.ValidContent)
    {
        return new(content!);
    }

    public static GetPost.Request CreateGetRequest(Guid? postId = null)
    {
        return new(postId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static DeletePost.Request CreateDeleteRequest(Guid? postId = null)
    {
        return new(postId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static GetUserPosts.Request CreateGetUserPostsRequest(Guid? userId = null)
    {
        return new(userId ?? ValidatorTestData.ValidPrimaryGuid, Cursor: null);
    }
}
