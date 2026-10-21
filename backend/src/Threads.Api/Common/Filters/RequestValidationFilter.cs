using FluentValidation;

namespace Threads.Api.Common.Filters;

public class RequestValidationFilter<TRequest> : IEndpointFilter
{
    private readonly ILogger<RequestValidationFilter<TRequest>> _logger;
    private readonly IValidator<TRequest>? _validator;

    public RequestValidationFilter(
        ILogger<RequestValidationFilter<TRequest>> logger,
        IValidator<TRequest>? validator
    )
    {
        _logger = logger;
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var requestName = typeof(TRequest).FullName?.Replace("+", ".");

        if (_validator == null)
        {
            _logger.LogInformation("{Request}: No validator is specified.", requestName);
            return await next.Invoke(context);
        }

        var requestDto = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (requestDto == null)
            throw new InvalidOperationException(
                $"Developer error: Parameter of type '{typeof(TRequest).Name}' was not found in the endpoint signature."
            );

        var result = await _validator.ValidateAsync(requestDto);

        if (!result.IsValid)
        {
            _logger.LogWarning("{Request}: Failed to validate.", requestName);
            return TypedResults.ValidationProblem(result.ToDictionary());
        }

        return await next.Invoke(context);
    }
}
