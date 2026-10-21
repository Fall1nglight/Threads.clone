using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Comments.Endpoints;

namespace Threads.Api.Features.Comments;

public class CommentRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var anonGroup = builder.MapGroup("/posts").AllowAnonymous();
        anonGroup.MapEndpoint<GetComments>();

        var protectedGroup = builder.MapGroup("/posts").RequireAuthorization();
        protectedGroup
            .MapEndpoint<CreateComment>()
            .MapEndpoint<UpdateComment>()
            .MapEndpoint<DeleteComment>();
    }
}
