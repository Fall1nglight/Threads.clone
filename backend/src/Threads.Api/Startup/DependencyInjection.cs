using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Threads.Api.Common.ExceptionHandlers;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;
using Threads.Api.Features.Auth.Services.JwtProvider;
using Threads.Api.Features.Auth.Services.RefreshToken;

namespace Threads.Api.Startup;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        AddPersistence(services, configuration);
        AddAuthOptions(services, configuration);
        AddAuthenticationAndAuthorization(services);
        AddExceptionHandling(services);
        AddSerilogLogging(services, configuration);
        AddValidation(services);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSql"));
        });
    }

    private static void AddAuthOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(options =>
        {
            configuration.GetSection(nameof(JwtOptions)).Bind(options);
        });

        services.Configure<RefreshTokenOptions>(options =>
        {
            configuration.GetSection(nameof(RefreshTokenOptions)).Bind(options);
        });

        services.ConfigureOptions<JwtBearerOptionsSetup>();
    }

    private static void AddAuthenticationAndAuthorization(IServiceCollection services)
    {
        services
            .AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme);

        services.AddAuthorization();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IRefreshTokenProvider, RefreshTokenProvider>();
        services.AddScoped<IRefreshTokenManager, RefreshTokenManager>();
    }

    private static void AddExceptionHandling(IServiceCollection services)
    {
        services.AddProblemDetails();

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        });

        services.AddExceptionHandler<JsonExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
    }

    private static void AddSerilogLogging(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog(
            (registeredServices, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(configuration)
                    .ReadFrom.Services(registeredServices);
            }
        );
    }

    private static void AddValidation(IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    }
}
