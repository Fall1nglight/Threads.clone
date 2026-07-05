using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Auth.Endpoints;

namespace Threads.Api.Features.Auth;

public class AuthRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var anonymousAuthRoute = builder.MapGroup("/auth").AllowAnonymous();
        anonymousAuthRoute.MapEndpoint<Login>().MapEndpoint<Signup>().MapEndpoint<RenewToken>();

        var authorizedAuthRoute = builder.MapGroup("/auth").RequireAuthorization();
        authorizedAuthRoute.MapEndpoint<Logout>().MapEndpoint<LogoutAll>();
    }
}
