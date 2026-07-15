using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Blocks.Endpoints;

public class GetBlockedUsers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("", Handle).WithSummary("Retrieves blocked users");
    }

    private static async Task<Ok<PagedResponse<BlockedUserDto>>> Handle(
        [AsParameters] PagedRequest request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = claimsPrincipal.GetUserId();
        var users = await db
            .UserBlocks.Where(block => block.BlockerId == currentUserId)
            .ToBlockedUserDto()
            .ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(users);
    }
}
