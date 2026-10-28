using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Threads.Api.Common;
using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Data.Users;

namespace Threads.Api.Features.Auth.Endpoints;

public class ChangePassword : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/change", Handle);
    }

    public record Request(string CurrentPassword, string NewPassword);

    public class UpdatePasswordValidator : AbstractValidator<Request>
    {
        public UpdatePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(100)
                .Matches("[A-Z]")
                .Matches("[a-z]")
                .Matches("[0-9]")
                .Matches("[^a-zA-Z0-9]");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(100)
                .Matches("[A-Z]")
                .Matches("[a-z]")
                .Matches("[0-9]")
                .Matches("[^a-zA-Z0-9]");
        }
    }

    private static async Task<Results<UnauthorizedHttpResult, ProblemHttpResult, NoContent>> Handle(
        Request request,
        UserManager<User> userManager,
        ClaimsPrincipal claimsPrincipal
    )
    {
        Guid loggedInUserId = claimsPrincipal.GetUserId();
        User? user = await userManager.FindByIdAsync(loggedInUserId.ToString());

        if (user == null)
            return TypedResults.Unauthorized();

        IdentityResult updateResult = await userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword
        );

        if (!updateResult.Succeeded)
        {
            string errorDetails = string.Join("; ", updateResult.Errors.Select(e => e.Description));

            return HttpResultFactory.Create(
                statusCode: StatusCodes.Status400BadRequest,
                detail: errorDetails
            );
        }

        return TypedResults.NoContent();
    }
}
