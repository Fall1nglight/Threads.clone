using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Threads.Api.Data.Shared;

namespace Threads.Api.Data.Blocks;

public class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
{
    public void Configure(EntityTypeBuilder<UserBlock> builder)
    {
        builder.ToTable(
            "UserBlocks",
            DbContextSchemas.Default,
            table =>
                table.HasCheckConstraint(
                    "CK_UserBlocks_BlockerId_NotBlockedId",
                    "\"BlockerId\" <> \"BlockedId\""
                )
        );

        builder.HasKey(x => new { x.BlockerId, x.BlockedId });

        builder.HasIndex(x => new
        {
            x.BlockerId,
            x.CreatedAtUtc,
            x.BlockedId,
        });

        builder.HasIndex(x => new { x.BlockedId, x.BlockerId });

        builder
            .HasOne(x => x.Blocker)
            .WithMany()
            .HasForeignKey(x => x.BlockerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Blocked)
            .WithMany()
            .HasForeignKey(x => x.BlockedId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
