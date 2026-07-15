using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Common.Pagination;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Users.Endpoints;

public class GetUsers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet("", Handle).WithSummary("Retrieves user profiles");
    }

    private static async Task<Ok<PagedResponse<UserProfileDto>>> Handle(
        [AsParameters] PagedRequest request,
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var usersQuery = db.Users.AsQueryable();

        if (claimsPrincipal.Identity?.IsAuthenticated == true)
        {
            var currentUserId = claimsPrincipal.GetUserId();
            usersQuery = usersQuery.WhereVisibleTo(currentUserId, db);
        }

        var users = await usersQuery.ToUserProfileDto().ToPagedResponse(request, cancellationToken);

        return TypedResults.Ok(users);
    }
}
