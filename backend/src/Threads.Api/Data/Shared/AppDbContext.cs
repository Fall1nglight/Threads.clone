using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Threads.Api.Data.Blocks;
using Threads.Api.Data.Comments;
using Threads.Api.Data.Follows;
using Threads.Api.Data.Likes;
using Threads.Api.Data.Posts;
using Threads.Api.Data.Tokens;
using Threads.Api.Data.Users;

namespace Threads.Api.Data.Shared;

public sealed class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<UserBlock> UserBlocks { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        builder.HasDefaultSchema(DbContextSchemas.Default);
        ConfigureIdentityTables(builder);
    }

    private void ConfigureIdentityTables(ModelBuilder builder)
    {
        builder.Entity<User>().ToTable(DbContextTableNames.Users, DbContextSchemas.Identity);

        builder
            .Entity<IdentityRole<Guid>>()
            .ToTable(DbContextTableNames.Roles, DbContextSchemas.Identity);

        builder
            .Entity<IdentityUserRole<Guid>>()
            .ToTable(DbContextTableNames.UserRoles, DbContextSchemas.Identity);

        builder
            .Entity<IdentityUserClaim<Guid>>()
            .ToTable(DbContextTableNames.UserClaims, DbContextSchemas.Identity);

        builder
            .Entity<IdentityUserLogin<Guid>>()
            .ToTable(DbContextTableNames.UserLogins, DbContextSchemas.Identity);

        builder
            .Entity<IdentityRoleClaim<Guid>>()
            .ToTable(DbContextTableNames.RoleClaims, DbContextSchemas.Identity);

        builder
            .Entity<IdentityUserToken<Guid>>()
            .ToTable(DbContextTableNames.UserTokens, DbContextSchemas.Identity);
    }
}
