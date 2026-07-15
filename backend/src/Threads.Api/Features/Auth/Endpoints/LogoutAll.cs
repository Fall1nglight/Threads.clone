using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Auth.Services.RefreshToken;

namespace Threads.Api.Features.Auth.Endpoints;

public class LogoutAll : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/logout-all", Handle).WithSummary("Logs out user from every session");
    }

    private static async Task<Ok> Handle(
        ClaimsPrincipal claimsPrincipal,
        IRefreshTokenManager rtManager,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = claimsPrincipal.GetUserId();
        await rtManager.LogoutGlobalAsync(currentUserId, cancellationToken);
        return TypedResults.Ok();
    }
}
