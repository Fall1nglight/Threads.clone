using Microsoft.AspNetCore.Identity;
using Threads.Api.Data.Posts;

namespace Threads.Api.Data.Users;

public class User : IdentityUser<Guid>
{
    public string? Bio { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; set; }
    public List<Post> Posts { get; set; } = [];
}
