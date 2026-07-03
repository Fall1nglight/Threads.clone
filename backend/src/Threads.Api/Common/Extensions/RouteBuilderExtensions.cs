using Threads.Api.Common.Filters;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Common.Extensions;

public static class RouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder builder)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(builder);
        return builder;
    }

    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder)
    {
        builder.AddEndpointFilter<RequestValidationFilter<TRequest>>().ProducesValidationProblem();
        return builder;
    }
}
