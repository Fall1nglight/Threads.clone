using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Posts.Endpoints;

namespace Threads.Api.Features.Posts;

public class PostRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var anonGroup = builder.MapGroup("/posts").AllowAnonymous();
        anonGroup.MapEndpoint<GetAnonymousFeedPosts>().MapEndpoint<GetPost>();

        var anonUserGroup = builder.MapGroup("/users");
        anonUserGroup.MapEndpoint<GetUserPosts>();

        var protectedGroup = builder.MapGroup("/posts").RequireAuthorization();
        protectedGroup
            .MapEndpoint<GetGlobalFeedPosts>()
            .MapEndpoint<GetMyFeedPosts>()
            .MapEndpoint<CreatePost>()
            .MapEndpoint<UpdatePost>()
            .MapEndpoint<DeletePost>();
    }
}
