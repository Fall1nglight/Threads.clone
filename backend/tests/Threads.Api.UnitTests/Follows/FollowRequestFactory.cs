using Threads.Api.Features.Follows.Endpoints;
using Threads.Api.Features.Follows.Endpoints.Incoming;
using Threads.Api.Features.Follows.Endpoints.Outgoing;
using Threads.Api.UnitTests.TestSupport;

namespace Threads.Api.UnitTests.Follows;

public static class FollowRequestFactory
{
    public static SendFollowRequest.Request CreateSendFollowRequest(Guid? targetUserId = null)
    {
        return new(targetUserId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static AcceptIncomingFollowRequest.Request CreateAcceptIncomingFollowRequest(
        Guid? followerId = null
    )
    {
        return new(followerId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static RejectIncomingFollowRequest.Request CreateRejectIncomingFollowRequest(
        Guid? followerId = null
    )
    {
        return new(followerId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static DeleteFollowing.Request CreateDeleteFollowingRequest(Guid? followedId = null)
    {
        return new(followedId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static DeleteFollower.Request CreateDeleteFollowerRequest(Guid? followerId = null)
    {
        return new(followerId ?? ValidatorTestData.ValidPrimaryGuid);
    }

    public static GetFollowers.Request CreateGetFollowersRequest(Guid? userId = null)
    {
        return new(userId ?? ValidatorTestData.ValidPrimaryGuid, Cursor: null);
    }

    public static GetFollowing.Request CreateGetFollowingRequest(Guid? userId = null)
    {
        return new(userId ?? ValidatorTestData.ValidPrimaryGuid, Cursor: null);
    }

    public static CancelOutgoingFollowRequest.Request CreateCancelOutgoingFollowRequest(
        Guid? followedId = null
    )
    {
        return new(followedId ?? ValidatorTestData.ValidPrimaryGuid);
    }
}
