using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Users.Endpoints;

namespace Threads.Api.Features.Users;

public class UserRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var anonGroup = builder.MapGroup("/users").AllowAnonymous();
        anonGroup.MapEndpoint<GetUser>().MapEndpoint<SearchUsers>();

        var protectedGroup = builder.MapGroup("/users").RequireAuthorization();
        protectedGroup.MapEndpoint<UpdateUser>().MapEndpoint<DeleteUser>();
    }
}
