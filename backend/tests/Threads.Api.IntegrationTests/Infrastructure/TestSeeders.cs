using Microsoft.AspNetCore.Identity;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;
using Threads.Api.Features.Auth.Services.RefreshToken;
using Threads.Api.IntegrationTests.Auth.TestSupport;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Comments.TestSupport;
using Threads.Api.IntegrationTests.Follows.TestSupport;
using Threads.Api.IntegrationTests.Likes.TestSupport;
using Threads.Api.IntegrationTests.Notifications.TestSupport;
using Threads.Api.IntegrationTests.Posts.TestSupport;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Infrastructure;

public sealed class TestSeeders
{
    public UserTestSeeder Users { get; }
    public PostTestSeeder Posts { get; }
    public FollowTestSeeder Follows { get; }
    public BlockTestSeeder Blocks { get; }
    public LikeTestSeeder Likes { get; }
    public CommentTestSeeder Comments { get; }
    public NotificationTestSeeder Notifications { get; }
    public RefreshTokenTestSeeder RefreshTokens { get; }

    public TestSeeders(
        AppDbContext db,
        UserManager<User> userManager,
        IRefreshTokenProvider refreshTokenProvider
    )
    {
        Users = new UserTestSeeder(userManager);
        Posts = new PostTestSeeder(db);
        Follows = new FollowTestSeeder(db);
        Blocks = new BlockTestSeeder(db);
        Likes = new LikeTestSeeder(db);
        Comments = new CommentTestSeeder(db);
        Notifications = new NotificationTestSeeder(db);
        RefreshTokens = new RefreshTokenTestSeeder(db, refreshTokenProvider);
    }
}
