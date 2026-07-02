namespace Threads.Api.IntegrationTests.Infrastructure;

[CollectionDefinition("Integration tests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory> { }
