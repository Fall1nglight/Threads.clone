using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Data.Users;
using Threads.Api.Features.Auth.Services.RefreshToken;
using Threads.Api.IntegrationTests.Auth.TestSupport;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Comments.TestSupport;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Likes.TestSupport;
using Threads.Api.IntegrationTests.Posts.TestSupport;

namespace Threads.Api.IntegrationTests.Users.TestSupport;

public abstract class UserIntegrationTestBase : IntegrationTestBase
{
    protected UserIntegrationTestBase(CustomWebApplicationFactory factory)
        : base(factory) { }

    protected UserTestSeeder CreateUserSeeder()
    {
        var userManager = Services.GetRequiredService<UserManager<User>>();
        return new UserTestSeeder(userManager);
    }

    protected BlockTestSeeder CreateBlockSeeder()
    {
        return new BlockTestSeeder(Db);
    }

    protected UserDeletionScenarioSeeder CreateUserDeletionScenarioSeeder()
    {
        var refreshTokenProvider = Services.GetRequiredService<IRefreshTokenProvider>();

        return new UserDeletionScenarioSeeder(
            CreateUserSeeder(),
            new PostTestSeeder(Db),
            new FollowTestSeeder(Db),
            CreateBlockSeeder(),
            new LikeTestSeeder(Db),
            new CommentTestSeeder(Db),
            new RefreshTokenTestSeeder(Db, refreshTokenProvider)
        );
    }
}
