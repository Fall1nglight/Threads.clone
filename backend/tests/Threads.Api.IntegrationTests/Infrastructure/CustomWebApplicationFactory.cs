using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Threads.Api.Data.Shared;

namespace Threads.Api.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string PostgresImage = "postgres:18.4";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder(PostgresImage)
        .WithDatabase("test-db")
        .WithUsername("test-user")
        .WithPassword("test-pwd")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // configures jwt options
        builder.ConfigureAppConfiguration(
            (_, configBuilder) =>
            {
                var jwtOptions = new Dictionary<string, string?>
                {
                    ["JwtOptions:Issuer"] = "ThreadsApi.Tests",
                    ["JwtOptions:Audience"] = "ThreadsApi.Tests",
                    ["JwtOptions:ExpirationInMinutes"] = "60",
                    ["JwtOptions:SecretKey"] =
                        "integration-test-secret-key-with-enough-bytes-for-hmac",
                    ["RefreshTokenOptions:LifetimeInDays"] = "7",
                    ["RefreshTokenOptions:MaxActiveTokensPerUser"] = "5",
                };

                configBuilder.AddInMemoryCollection(jwtOptions);
            }
        );

        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.FirstOrDefault(s =>
                s.ServiceType == typeof(DbContextOptions<AppDbContext>)
            );

            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            services.AddDbContext<AppDbContext>(optionsBuilder =>
            {
                optionsBuilder.UseNpgsql(_dbContainer.GetConnectionString());
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
