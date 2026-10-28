using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Resend;
using Threads.Api.Common;
using Threads.Api.Common.Email;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Data.Users;

namespace Threads.Api.Features.Auth.Endpoints.EmailVerification;

public class SendEmailVerificationCode : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/send", Handle);
    }

    private static async Task<Results<UnauthorizedHttpResult, ProblemHttpResult, NoContent>> Handle(
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

        string code = await userManager.GenerateEmailConfirmationTokenAsync(user);

        ResendResponse<Guid> sendResult = await emailSender.SendConfirmEmailAsync(
            user.Email!,
            code
        );

        if (!sendResult.Success)
        {
            HttpResultFactory.Create(
                StatusCodes.Status503ServiceUnavailable,
                "The verification email could not be sent."
            );
        }

        return TypedResults.NoContent();
    }
}
