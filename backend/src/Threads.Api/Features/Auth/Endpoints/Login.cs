using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Threads.Api.Common;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Data.Users;
using Threads.Api.Features.Auth.Services.JwtProvider;
using Threads.Api.Features.Auth.Services.RefreshToken;

namespace Threads.Api.Features.Auth.Endpoints;

public class Login : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/login", Handle).WithValidation<Request>();
    }

    public record Request(string Email, string Password);

    public record Response(string AccessToken, string RefreshToken);

    public class LoginValidator : AbstractValidator<Request>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]")
                .Matches("[a-z]")
                .Matches("[0-9]")
                .Matches("[^a-zA-Z0-9]");
        }
    }

    private static async Task<Results<Ok<Response>, ProblemHttpResult>> Handle(
        [FromBody] Request request,
        UserManager<User> userManager,
        IJwtProvider jwtProvider,
        IRefreshTokenManager rtManager,
        CancellationToken cancellationToken
    )
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return CustomResults.Unauthorized("The provided email is incorrect");

        var isValidPassword = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
            return CustomResults.Unauthorized("The provided password is incorrect");

        var accessToken = await jwtProvider.GenerateAsync(user);
        var refreshToken = await rtManager.GenerateAsync(user.Id, cancellationToken);
        var response = new Response(accessToken, refreshToken);

        return TypedResults.Ok(response);
    }
}
