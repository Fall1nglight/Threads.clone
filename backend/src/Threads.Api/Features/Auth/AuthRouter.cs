using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Auth.Endpoints;

namespace Threads.Api.Features.Auth;

public class AuthRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var anonGroup = builder.MapGroup("/auth").AllowAnonymous();
        anonGroup.MapEndpoint<Login>().MapEndpoint<Signup>().MapEndpoint<RenewToken>();

        var protectedGroup = builder.MapGroup("/auth").RequireAuthorization();
        protectedGroup.MapEndpoint<Logout>().MapEndpoint<LogoutAll>();
    }
}
