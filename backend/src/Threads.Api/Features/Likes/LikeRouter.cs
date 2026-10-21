using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Likes.Endpoints;

namespace Threads.Api.Features.Likes;

public class LikeRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var protectedGroup = builder.MapGroup("/posts").RequireAuthorization();
        protectedGroup.MapEndpoint<LikePost>().MapEndpoint<UnlikePost>().MapEndpoint<GetLikes>();
    }
}
