using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Posts.TestSupport;

public sealed class PostTestSeeder
{
    private readonly AppDbContext _db;

    public PostTestSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Post> CreatePostAsync(
        User owner,
        string content,
        DateTime? createdAtUtc = null,
        bool isDeleted = false,
        Guid? id = null
    )
    {
        var post = CreatePost(new PostSeedData(owner, content, createdAtUtc, isDeleted, id));

        await _db.Posts.AddAsync(post);
        await _db.SaveChangesAsync();
        return post;
    }

    public async Task<IReadOnlyList<Post>> CreatePostsAsync(IReadOnlyCollection<PostSeedData> seeds)
    {
        var posts = seeds.Select(CreatePost).ToList();

        await _db.Posts.AddRangeAsync(posts);
        await _db.SaveChangesAsync();
        return posts;
    }

    private static Post CreatePost(PostSeedData seed)
    {
        return new Post
        {
            Id = seed.Id ?? Guid.NewGuid(),
            UserId = seed.Owner.Id,
            Content = seed.Content,
            CreatedAtUtc = seed.CreatedAtUtc ?? IntegrationTestData.BaseTime,
            IsDeleted = seed.IsDeleted,
        };
    }
}

public sealed record PostSeedData(
    User Owner,
    string Content,
    DateTime? CreatedAtUtc = null,
    bool IsDeleted = false,
    Guid? Id = null
);
