using Threads.Api.Data.Likes;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Likes.TestSupport;

public sealed class LikeTestSeeder
{
    private readonly AppDbContext _db;

    public LikeTestSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PostLike> CreatePostLikeAsync(
        User user,
        Post post,
        DateTime? createdAtUtc = null
    )
    {
        var postLike = new PostLike
        {
            UserId = user.Id,
            PostId = post.Id,
            CreatedAtUtc = createdAtUtc ?? IntegrationTestData.BaseTime,
        };

        await _db.PostLikes.AddAsync(postLike);
        await _db.SaveChangesAsync();
        return postLike;
    }

    public async Task<IReadOnlyList<PostLike>> CreatePostLikesAsync(
        IReadOnlyCollection<PostLikeSeedData> seeds
    )
    {
        var postLikes = seeds
            .Select(seed => new PostLike
            {
                UserId = seed.User.Id,
                PostId = seed.Post.Id,
                CreatedAtUtc = seed.CreatedAtUtc ?? IntegrationTestData.BaseTime,
            })
            .ToList();

        await _db.PostLikes.AddRangeAsync(postLikes);
        await _db.SaveChangesAsync();
        return postLikes;
    }
}

public sealed record PostLikeSeedData(User User, Post Post, DateTime? CreatedAtUtc = null);
