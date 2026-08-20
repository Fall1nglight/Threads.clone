using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Follows.Endpoints;

namespace Threads.Api.Features.Follows;

public class FollowRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var authorizedFollowRoute = builder.MapGroup("/follows").RequireAuthorization();

        authorizedFollowRoute
            .MapEndpoint<SendFollowRequest>()
            .MapEndpoint<AcceptIncomingFollowRequest>()
            .MapEndpoint<RejectIncomingFollowRequest>()
            .MapEndpoint<GetIncomingFollowRequests>()
            .MapEndpoint<GetOutgoingFollowRequests>()
            .MapEndpoint<GetFollowers>()
            .MapEndpoint<GetFollowing>()
            .MapEndpoint<RemoveFollower>()
            .MapEndpoint<DeleteFollowing>();

        var userSpecificAuthorizedRoute = builder.MapGroup("/users").RequireAuthorization();
        userSpecificAuthorizedRoute.MapEndpoint<GetUserFollowers>().MapEndpoint<GetUserFollowing>();
    }
}
