using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Posts.TestSupport;

public abstract class PostIntegrationTestBase : IntegrationTestBase
{
    protected PostIntegrationTestBase(CustomWebApplicationFactory factory)
        : base(factory) { }

    protected PostTestSeeder CreatePostSeeder()
    {
        var userManager = Services.GetRequiredService<UserManager<User>>();
        return new PostTestSeeder(Db, userManager);
    }
}
