using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Users.Endpoints;

namespace Threads.Api.Features.Users;

public class UserRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var anonymousUsersRoute = builder.MapGroup("/users").AllowAnonymous();

        anonymousUsersRoute.MapEndpoint<GetUsers>().MapEndpoint<GetUser>();

        var authorizedUsersRoute = builder.MapGroup("/users").RequireAuthorization();

        authorizedUsersRoute.MapEndpoint<DeleteUser>();
    }
}
