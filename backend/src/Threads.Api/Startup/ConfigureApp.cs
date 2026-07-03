using System.Reflection;
using Serilog;
using Threads.Api.Data.Shared.Interfaces;

namespace Threads.Api.Startup;

public static class ConfigureApp
{
    public static IApplicationBuilder UseMiddlewares(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var assembly = typeof(Program).Assembly;

        var routers = assembly
            .GetTypes()
            .Where(type =>
                type.IsClass && type.IsAssignableTo(typeof(IEndpointRouter)) && !type.IsAbstract
            );

        foreach (var router in routers)
        {
            var mapMethod = router.GetMethod(
                nameof(IEndpointRouter.MapRouter),
                BindingFlags.Public | BindingFlags.Static
            );

            if (mapMethod == null)
                continue;

            mapMethod.Invoke(null, [builder]);
        }

        return builder;
    }
}
