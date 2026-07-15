using Threads.Api.Data.Likes;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Blocks.TestSupport;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Posts.TestSupport;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Likes.TestSupport;

public sealed class LikeListScenarioSeeder
{
    private readonly UserTestSeeder _userSeeder;
    private readonly PostTestSeeder _postSeeder;
    private readonly LikeTestSeeder _likeSeeder;
    private readonly BlockTestSeeder _blockSeeder;

    public LikeListScenarioSeeder(
        UserTestSeeder userSeeder,
        PostTestSeeder postSeeder,
        LikeTestSeeder likeSeeder,
        BlockTestSeeder blockSeeder
    )
    {
        _userSeeder = userSeeder;
        _postSeeder = postSeeder;
        _likeSeeder = likeSeeder;
        _blockSeeder = blockSeeder;
    }

    public async Task<LikeListSeed> SeedAsync()
    {
        var requester = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var postOwner = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.BobUsername
        );
        var visibleLiker = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.CarolUsername
        );
        var blockedLiker = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.HenryUsername
        );
        var blockingLiker = await _userSeeder.CreateUserAsync(username: "blocking-liker");
        var post = await _postSeeder.CreatePostAsync(postOwner, IntegrationTestData.ValidContent);
        var likes = await _likeSeeder.CreatePostLikesAsync(
            new List<PostLikeSeedData>
            {
                new(requester, post, IntegrationTestData.BaseTime.AddMinutes(1)),
                new(visibleLiker, post, IntegrationTestData.BaseTime.AddMinutes(2)),
                new(blockedLiker, post, IntegrationTestData.BaseTime.AddMinutes(3)),
                new(blockingLiker, post, IntegrationTestData.BaseTime.AddMinutes(4)),
            }
        );

        await _blockSeeder.CreateUserBlockAsync(requester, blockedLiker);
        await _blockSeeder.CreateUserBlockAsync(blockingLiker, requester);

        return new(requester, post, visibleLiker, likes[0], likes[1]);
    }

    public async Task<LikeBulkSeed> SeedBulkAsync(int count)
    {
        var requester = await _userSeeder.CreateUserAsync(
            username: IntegrationTestData.AliceUsername
        );
        var post = await _postSeeder.CreatePostAsync(requester, IntegrationTestData.ValidContent);
        var users = new List<User>(count);

        for (var index = 0; index < count; index++)
            users.Add(await _userSeeder.CreateUserAsync(username: $"bulk-liker-{index:D3}"));

        var likes = await _likeSeeder.CreatePostLikesAsync(
            users
                .Select(
                    (user, index) =>
                        new PostLikeSeedData(
                            user,
                            post,
                            IntegrationTestData.BaseTime.AddMinutes(index)
                        )
                )
                .ToList()
        );

        return new(requester, post, users, likes);
    }
}

public sealed record LikeListSeed(
    User Requester,
    Post Post,
    User VisibleLiker,
    PostLike RequesterLike,
    PostLike VisibleLike
);

public sealed record LikeBulkSeed(
    User Requester,
    Post Post,
    IReadOnlyList<User> Users,
    IReadOnlyList<PostLike> Likes
);
