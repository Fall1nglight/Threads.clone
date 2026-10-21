using Threads.Api.Features.Likes.Endpoints;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Likes;

public static class LikeRequestFactory
{
    public static LikePost.Request CreateLikePostRequest(Guid? postId = null)
    {
        return new(postId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static UnlikePost.Request CreateUnlikePostRequest(Guid? postId = null)
    {
        return new(postId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static GetLikes.Request CreateGetLikesRequest(Guid? postId = null)
    {
        return new(postId ?? ValidatorTestData.ValidPrimaryGuid, Cursor: null);
    }
}
