using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Auth.Services.RefreshToken;

namespace Threads.Api.Features.Auth.Endpoints;

public class Logout : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost("/logout", Handle)
            .WithValidation<Request>()
            .WithSummary("Logs out user from the current session");
    }

    public record Request(string RefreshToken);

    public class LogoutValidator : AbstractValidator<Request>
    {
        public LogoutValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().Length(44);
        }
    }

    private static async Task<Ok> Handle(
        [FromBody] Request request,
        IRefreshTokenManager rtManager,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = claimsPrincipal.GetUserId();
        await rtManager.LogoutSingleAsync(currentUserId, request.RefreshToken, cancellationToken);
        return TypedResults.Ok();
    }
}
