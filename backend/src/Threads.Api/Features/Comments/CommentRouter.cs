using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Comments.Endpoints;

namespace Threads.Api.Features.Comments;

public class CommentRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var authorizedCommentRoute = builder.MapGroup("/posts").RequireAuthorization();

        authorizedCommentRoute
            .MapEndpoint<CreateComment>()
            .MapEndpoint<GetPostComments>()
            .MapEndpoint<UpdateComment>()
            .MapEndpoint<DeleteComment>();
    }
}
