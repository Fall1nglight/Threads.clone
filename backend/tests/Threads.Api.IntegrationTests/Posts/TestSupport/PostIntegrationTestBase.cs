using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Comments.TestSupport;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Likes.TestSupport;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Posts.TestSupport;

public abstract class PostIntegrationTestBase : IntegrationTestBase
{
    protected PostIntegrationTestBase(CustomWebApplicationFactory factory)
        : base(factory) { }

    protected UserTestSeeder CreateUserSeeder()
    {
        var userManager = Services.GetRequiredService<UserManager<User>>();
        return new UserTestSeeder(userManager);
    }

    protected PostTestSeeder CreatePostSeeder()
    {
        return new PostTestSeeder(Db);
    }

    protected BlockTestSeeder CreateBlockSeeder()
    {
        return new BlockTestSeeder(Db);
    }

    protected PostVisibilityScenarioSeeder CreatePostVisibilityScenarioSeeder()
    {
        return new PostVisibilityScenarioSeeder(
            CreateUserSeeder(),
            CreatePostSeeder(),
            new FollowTestSeeder(Db)
        );
    }

    protected PostPaginationScenarioSeeder CreatePostPaginationScenarioSeeder()
    {
        return new PostPaginationScenarioSeeder(CreateUserSeeder(), CreatePostSeeder());
    }

    protected PostEngagementVisibilityScenarioSeeder CreatePostEngagementVisibilityScenarioSeeder()
    {
        return new PostEngagementVisibilityScenarioSeeder(
            CreatePostVisibilityScenarioSeeder(),
            CreateBlockSeeder(),
            new LikeTestSeeder(Db),
            new CommentTestSeeder(Db)
        );
    }
}
