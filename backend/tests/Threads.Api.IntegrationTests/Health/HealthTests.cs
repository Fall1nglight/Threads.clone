using System.Net;
using FluentAssertions;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Health;

public sealed class HealthTests : IntegrationTestBase
{
    private const string HealthRoute = "health/api";

    public HealthTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Health_ShouldReturnOk_WhenRequesterIsAnonymous()
    {
        // Arrange
        var client = CreateAnonymousClient();

        // Act
        var response = await client.GetAsync(HealthRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_ShouldReturnOk_WhenRequesterIsAuthenticated()
    {
        // Arrange
        var user = await Seeders.Users.CreateUserAsync("health-check-user");
        var client = await CreateAuthenticatedClientAsync(user);

        // Act
        var response = await client.GetAsync(HealthRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
