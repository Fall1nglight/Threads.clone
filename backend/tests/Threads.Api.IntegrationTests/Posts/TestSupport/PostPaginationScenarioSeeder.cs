using Threads.Api.Data.Posts;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Infrastructure;
using Threads.Api.IntegrationTests.Users.TestSupport;

namespace Threads.Api.IntegrationTests.Posts.TestSupport;

public sealed class PostPaginationScenarioSeeder
{
    private readonly PostTestSeeder _postSeeder;
    private readonly UserTestSeeder _userSeeder;

    public PostPaginationScenarioSeeder(UserTestSeeder userSeeder, PostTestSeeder postSeeder)
    {
        _userSeeder = userSeeder;
        _postSeeder = postSeeder;
    }

    public async Task<PostBulkSeed> SeedBulkPublicPostsAsync(int count)
    {
        var user = await _userSeeder.CreateUserAsync(
            username: $"{PostTestData.BulkUsernamePrefix}-{Guid.NewGuid():N}"
        );
        var postSeeds = Enumerable
            .Range(0, count)
            .Select(index => new PostSeedData(
                user,
                $"{PostTestData.BulkPostContentPrefix} {index}",
                IntegrationTestData.BaseTime.AddMinutes(index)
            ))
            .ToList();
        var posts = await _postSeeder.CreatePostsAsync(postSeeds);

        return new PostBulkSeed(user, posts);
    }

    public async Task<SameTimestampPostSeed> SeedSameTimestampPostsAsync()
    {
        var user = await _userSeeder.CreateUserAsync(
            username: $"{PostTestData.SameTimestampUsernamePrefix}-{Guid.NewGuid():N}"
        );
        var timestamp = IntegrationTestData.BaseTime.AddHours(1);
        var posts = await _postSeeder.CreatePostsAsync(
            new List<PostSeedData>
            {
                new(
                    user,
                    PostTestData.SameTimestampLowerPostContent,
                    timestamp,
                    Id: PostTestData.LowerTieBreakerPostId
                ),
                new(
                    user,
                    PostTestData.SameTimestampHigherPostContent,
                    timestamp,
                    Id: PostTestData.HigherTieBreakerPostId
                ),
            }
        );

        return new SameTimestampPostSeed(user, posts[0], posts[1]);
    }
}

public sealed record PostBulkSeed(User User, IReadOnlyList<Post> Posts);

public sealed record SameTimestampPostSeed(User User, Post LowerIdPost, Post HigherIdPost);
