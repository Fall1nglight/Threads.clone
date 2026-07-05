using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Data.Users;
using Threads.Api.Features.Auth.Services.JwtProvider;
using Threads.Api.Features.Auth.Services.RefreshToken;

namespace Threads.Api.Features.Auth.Endpoints;

public class Signup : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/signup", Handle).WithSummary("Registers user").WithValidation<Request>();
    }

    public record Request(
        string Email,
        string Username,
        string Password,
        bool IsPrivate,
        string? Bio
    );

    public record Response(string AccessToken, string RefreshToken);

    public class SignupValidator : AbstractValidator<Request>
    {
        public SignupValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

            RuleFor(x => x.Username)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(30)
                .Matches("^[a-zA-Z0-9_.]+$");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(100)
                .Matches("[A-Z]")
                .Matches("[a-z]")
                .Matches("[0-9]")
                .Matches("[^a-zA-Z0-9]");

            RuleFor(x => x.Bio).MaximumLength(300);
        }
    }

    private static async Task<Results<Ok<Response>, ProblemHttpResult>> Handle(
        [FromBody] Request request,
        AppDbContext db,
        UserManager<User> userManager,
        IJwtProvider jwtProvider,
        IRefreshTokenManager rtManager,
        CancellationToken cancellationToken
    )
    {
        var newUser = new User()
        {
            Email = request.Email,
            UserName = request.Username,
            Bio = request.Bio,
            IsPrivate = request.IsPrivate,
            CreatedAtUtc = DateTime.UtcNow,
        };

        IdentityResult result = await userManager.CreateAsync(newUser, request.Password);

        if (!result.Succeeded)
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Signup failed",
                detail: "The provided username or email is already in use"
            );

        var accessToken = await jwtProvider.GenerateAsync(newUser);
        var refreshToken = await rtManager.GenerateAsync(newUser.Id, cancellationToken);
        var response = new Response(accessToken, refreshToken);

        return TypedResults.Ok(response);
    }
}
