using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Resend;
using Threads.Api.Common;
using Threads.Api.Common.Email;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Data.Users;

namespace Threads.Api.Features.Auth.Endpoints.EmailVerification;

public class VerifyEmailVerificationCode : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/verify", Handle).WithValidation<Request>();
    }

    public record Request(string Code);

    public class VerifyEmailVerificationCodeValidator : AbstractValidator<Request>
    {
        public VerifyEmailVerificationCodeValidator()
        {
            RuleFor(x => x.Code)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeCanonicalBase64Token)
                .WithMessage("'{PropertyName}' must be a valid email verification token.");
        }

        private static bool BeCanonicalBase64Token(string? code)
        {
            if (string.IsNullOrEmpty(code))
                return false;

            try
            {
                byte[] decodedToken = Convert.FromBase64String(code);
                return Convert.ToBase64String(decodedToken) == code;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }

    private static async Task<Results<UnauthorizedHttpResult, ProblemHttpResult, NoContent>> Handle(
        Request request,
        UserManager<User> userManager,
        IEmailSender emailSender,
        ClaimsPrincipal claimsPrincipal
    )
    {
        Guid loggedInUserId = claimsPrincipal.GetUserId();
        User? user = await userManager.FindByIdAsync(loggedInUserId.ToString());

        if (user == null)
            return TypedResults.Unauthorized();

        bool isVerified = await userManager.IsEmailConfirmedAsync(user);

        if (isVerified)
            return HttpResultFactory.Create(
                StatusCodes.Status409Conflict,
                "The email address associated with this account has already been verified."
            );

        IdentityResult confirmResult = await userManager.ConfirmEmailAsync(user, request.Code);
        if (!confirmResult.Succeeded)
        {
            string errorDetails = string.Join(
                "; ",
                confirmResult.Errors.Select(e => e.Description)
            );

            return HttpResultFactory.Create(
                statusCode: StatusCodes.Status400BadRequest,
                detail: errorDetails
            );
        }

        // notify user about successfull confirmation
        ResendResponse<Guid> sendResult = await emailSender.SendEmailConfirmationSucceededAsync(
            user.Email!
        );

        // if it fails we have to store it as a job to be executed later
        if (!sendResult.Success)
        {
            // todo | implement background worker to send message later
        }

        return TypedResults.NoContent();
    }
}
