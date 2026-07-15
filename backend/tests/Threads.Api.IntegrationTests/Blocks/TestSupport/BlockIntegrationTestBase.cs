using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Blocks.TestSupport;

public abstract class BlockIntegrationTestBase : IntegrationTestBase
{
    protected BlockIntegrationTestBase(CustomWebApplicationFactory factory)
        : base(factory) { }

    protected UserTestSeeder CreateUserSeeder()
    {
        var userManager = Services.GetRequiredService<UserManager<User>>();
        return new UserTestSeeder(userManager);
    }

    protected BlockTestSeeder CreateBlockSeeder() => new(Db);

    protected FollowTestSeeder CreateFollowSeeder() => new(Db);

    protected BlockRelationshipScenarioSeeder CreateBlockRelationshipScenarioSeeder() =>
        new(CreateUserSeeder(), CreateBlockSeeder(), CreateFollowSeeder());
}
