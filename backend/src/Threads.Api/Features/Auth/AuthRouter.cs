using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Auth.Endpoints;
using Threads.Api.Features.Auth.Endpoints.EmailVerification;

namespace Threads.Api.Features.Auth;

public class AuthRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var anonGroup = builder.MapGroup("/auth").AllowAnonymous();
        anonGroup.MapEndpoint<Login>().MapEndpoint<Signup>().MapEndpoint<RenewToken>();

        var protectedGroup = builder.MapGroup("/auth").RequireAuthorization();
        protectedGroup.MapEndpoint<Logout>().MapEndpoint<LogoutAll>();

        var protectedPasswordGroup = builder.MapGroup("/auth/password").RequireAuthorization();
        protectedPasswordGroup.MapEndpoint<ChangePassword>();

        var protectedEmailVerificationGroup = builder
            .MapGroup("/auth/email-verification")
            .RequireAuthorization();

        protectedEmailVerificationGroup
            .MapEndpoint<SendEmailVerificationCode>()
            .MapEndpoint<VerifyEmailVerificationCode>();
    }
}
