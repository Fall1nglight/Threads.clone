using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Posts.TestSupport;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Comments.TestSupport;

public abstract class CommentIntegrationTestBase : IntegrationTestBase
{
    protected CommentIntegrationTestBase(CustomWebApplicationFactory factory)
        : base(factory) { }

    protected UserTestSeeder CreateUserSeeder()
    {
        var userManager = Services.GetRequiredService<UserManager<User>>();
        return new UserTestSeeder(userManager);
    }

    protected PostTestSeeder CreatePostSeeder() => new(Db);

    protected CommentTestSeeder CreateCommentSeeder() => new(Db);

    protected BlockTestSeeder CreateBlockSeeder() => new(Db);

    protected PostVisibilityScenarioSeeder CreatePostVisibilityScenarioSeeder() =>
        new(CreateUserSeeder(), CreatePostSeeder(), new FollowTestSeeder(Db));

    protected CommentListScenarioSeeder CreateCommentListScenarioSeeder() =>
        new(CreateUserSeeder(), CreatePostSeeder(), CreateCommentSeeder(), CreateBlockSeeder());
}
