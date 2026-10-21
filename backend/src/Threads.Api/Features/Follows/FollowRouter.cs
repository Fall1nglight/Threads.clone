using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Follows.Endpoints;
using Threads.Api.Features.Follows.Endpoints.Incoming;
using Threads.Api.Features.Follows.Endpoints.Outgoing;

namespace Threads.Api.Features.Follows;

public class FollowRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var protectedGroup = builder.MapGroup("/me").RequireAuthorization();
        protectedGroup
            .MapEndpoint<SendFollowRequest>()
            .MapEndpoint<GetIncomingFollowRequests>()
            .MapEndpoint<AcceptIncomingFollowRequest>()
            .MapEndpoint<RejectIncomingFollowRequest>()
            .MapEndpoint<GetOutgoingFollowRequests>()
            .MapEndpoint<CancelOutgoingFollowRequest>()
            .MapEndpoint<DeleteFollower>()
            .MapEndpoint<DeleteFollowing>();

        var anonUserGroup = builder.MapGroup("/users").AllowAnonymous();
        anonUserGroup.MapEndpoint<GetFollowers>().MapEndpoint<GetFollowing>();
    }
}
