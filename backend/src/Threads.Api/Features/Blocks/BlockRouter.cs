using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Blocks.Endpoints;

namespace Threads.Api.Features.Blocks;

public class BlockRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var protectedGroup = builder.MapGroup("/blocks").RequireAuthorization();
        protectedGroup
            .MapEndpoint<BlockUser>()
            .MapEndpoint<UnblockUser>()
            .MapEndpoint<GetBlockedUsers>();
    }
}
