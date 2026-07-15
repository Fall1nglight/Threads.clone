using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Posts.TestSupport;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Likes.TestSupport;

public abstract class LikeIntegrationTestBase : IntegrationTestBase
{
    protected LikeIntegrationTestBase(CustomWebApplicationFactory factory)
        : base(factory) { }

    protected LikeTestSeeder CreateLikeSeeder()
    {
        return new LikeTestSeeder(Db);
    }

    protected BlockTestSeeder CreateBlockSeeder()
    {
        return new BlockTestSeeder(Db);
    }

    protected UserTestSeeder CreateUserSeeder()
    {
        var userManager = Services.GetRequiredService<UserManager<User>>();
        return new UserTestSeeder(userManager);
    }

    protected PostTestSeeder CreatePostSeeder() => new(Db);

    protected PostVisibilityScenarioSeeder CreatePostVisibilityScenarioSeeder()
    {
        var userManager = Services.GetRequiredService<UserManager<User>>();

        return new PostVisibilityScenarioSeeder(
            new UserTestSeeder(userManager),
            new PostTestSeeder(Db),
            new FollowTestSeeder(Db)
        );
    }

    protected LikeListScenarioSeeder CreateLikeListScenarioSeeder() =>
        new(CreateUserSeeder(), CreatePostSeeder(), CreateLikeSeeder(), CreateBlockSeeder());
}
