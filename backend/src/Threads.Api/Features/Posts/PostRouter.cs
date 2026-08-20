using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Posts.Endpoints;

namespace Threads.Api.Features.Posts;

public class PostRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var anonymousPostsRoute = builder.MapGroup("/posts");

        anonymousPostsRoute.MapEndpoint<GetAnonymousFeedPosts>();

        var authorizedPostsRoute = builder.MapGroup("/posts").RequireAuthorization();

        authorizedPostsRoute
            .MapEndpoint<GetGlobalFeedPosts>()
            .MapEndpoint<GetMyFeedPosts>()
            .MapEndpoint<GetPost>()
            .MapEndpoint<GetUserPosts>()
            .MapEndpoint<CreatePost>()
            .MapEndpoint<UpdatePost>()
            .MapEndpoint<DeletePost>();
    }
}
