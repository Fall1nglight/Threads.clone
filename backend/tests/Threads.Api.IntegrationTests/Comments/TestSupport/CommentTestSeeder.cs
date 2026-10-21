using Threads.Api.Data.Comments;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Shared;
using Threads.Api.Data.Users;
using Threads.Api.IntegrationTests.Infrastructure;

namespace Threads.Api.IntegrationTests.Comments.TestSupport;

public sealed class CommentTestSeeder
{
    private readonly AppDbContext _db;

    public CommentTestSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Comment> CreateCommentAsync(
        User user,
        Post post,
        string content,
        DateTime? createdAtUtc = null
    )
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PostId = post.Id,
            Content = content,
            CreatedAtUtc = createdAtUtc ?? IntegrationTestData.BaseTime,
        };

        await _db.Comments.AddAsync(comment);
        await _db.SaveChangesAsync();
        return comment;
    }

    public async Task<IReadOnlyList<Comment>> CreateCommentsAsync(
        IReadOnlyCollection<CommentSeedData> seeds
    )
    {
        var comments = seeds
            .Select(seed => new Comment
            {
                Id = Guid.NewGuid(),
                UserId = seed.User.Id,
                PostId = seed.Post.Id,
                Content = seed.Content,
                CreatedAtUtc = seed.CreatedAtUtc ?? IntegrationTestData.BaseTime,
            })
            .ToList();

        await _db.Comments.AddRangeAsync(comments);
        await _db.SaveChangesAsync();
        return comments;
    }
}

public sealed record CommentSeedData(
    User User,
    Post Post,
    string Content,
    DateTime? CreatedAtUtc = null
);
