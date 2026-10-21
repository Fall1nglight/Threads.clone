using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Features.Notifications.Endpoints;

public class GetUnseenNotificationCount : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("/unseen-count", Handle)
            .WithSummary("Retrieves the unseen notification count for the current user");
    }

    private static async Task<Ok<UnseenNotificationCountDto>> Handle(
        AppDbContext db,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var loggedInUserId = claimsPrincipal.GetUserId();
        var count = await db
            .Notifications.WhereVisibleTo(loggedInUserId, db)
            .CountAsync(notification => notification.SeenAtUtc == null, cancellationToken);

        return TypedResults.Ok(new UnseenNotificationCountDto(count));
    }
}
