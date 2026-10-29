using Threads.Api.Common.Extensions;
using Threads.Api.Data.Shared.Interfaces;
using Threads.Api.Features.Health.Endpoints;

namespace Threads.Api.Features.Health;

public sealed class HealthRouter : IEndpointRouter
{
    public static void MapRouter(IEndpointRouteBuilder builder)
    {
        var anonGroup = builder.MapGroup("/health").AllowAnonymous();
        anonGroup.MapEndpoint<CheckApiHealth>();
    }
}
