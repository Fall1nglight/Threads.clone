using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Follows.TestSupport;

public abstract class FollowIntegrationTestBase : IntegrationTestBase
{
    protected FollowIntegrationTestBase(CustomWebApplicationFactory factory)
        : base(factory) { }

    protected UserTestSeeder CreateUserSeeder()
    {
        var userManager = Services.GetRequiredService<UserManager<User>>();
        return new UserTestSeeder(userManager);
    }

    protected FollowTestSeeder CreateFollowSeeder() => new(Db);

    protected BlockTestSeeder CreateBlockSeeder() => new(Db);

    protected FollowRelationshipScenarioSeeder CreateFollowRelationshipScenarioSeeder() =>
        new(CreateUserSeeder(), CreateFollowSeeder(), CreateBlockSeeder());
}
