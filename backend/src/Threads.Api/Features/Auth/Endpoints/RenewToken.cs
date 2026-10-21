using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Auth.Services.JwtProvider;
using Threads.Api.Features.Auth.Services.RefreshToken;

namespace Threads.Api.Features.Auth.Endpoints;

public class RenewToken : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPost("/renew-token", Handle)
            .WithValidation<Request>()
            .WithSummary("Provides new refresh and access tokens");
    }

    public record Request(string RefreshToken);

    public record Response(string AccessToken, string RefreshToken);

    public class RenewTokenValidator : AbstractValidator<Request>
    {
        public RenewTokenValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().Length(44);
        }
    }

    private static async Task<Results<Ok<Response>, BadRequest>> Handle(
        [FromBody] Request request,
        IJwtProvider jwtProvider,
        IRefreshTokenManager rtManager,
        CancellationToken cancellationToken
    )
    {
        TokenRotationResult result = await rtManager.RotateAsync(
            request.RefreshToken,
            cancellationToken
        );

        if (!result.Succeeded)
            return TypedResults.BadRequest();

        var accessToken = await jwtProvider.GenerateAsync(result.User);
        var response = new Response(accessToken, result.PlainToken);

        return TypedResults.Ok(response);
    }
}
