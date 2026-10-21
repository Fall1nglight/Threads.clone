using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Threads.Api.Data.Shared;

namespace Threads.Api.Data.Notifications;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable(
            "Notifications",
            DbContextSchemas.Default,
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Notifications_SubjectKind_SubjectId",
                    "(\"SubjectKind\" IS NULL AND \"SubjectId\" IS NULL) OR "
                        + "(\"SubjectKind\" IS NOT NULL AND \"SubjectId\" IS NOT NULL)"
                );
                table.HasCheckConstraint(
                    "CK_Notifications_TriggeredByUserId_NotNotifiedUserId",
                    "\"TriggeredByUserId\" <> \"NotifiedUserId\""
                );
                table.HasCheckConstraint(
                    "CK_Notifications_SeenAtUtc_NotBeforeTriggeredAtUtc",
                    "\"SeenAtUtc\" IS NULL OR \"SeenAtUtc\" >= \"TriggeredAtUtc\""
                );
            }
        );

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.SubjectKind).HasConversion<string>().HasMaxLength(32);

        builder
            .HasIndex(x => new
            {
                x.NotifiedUserId,
                x.TriggeredAtUtc,
                x.Id,
            })
            .IsDescending(false, true, true);

        builder
            .HasIndex(x => new
            {
                x.NotifiedUserId,
                x.Type,
                x.TriggeredAtUtc,
                x.Id,
            })
            .IsDescending(false, false, true, true);

        builder.HasIndex(x => x.TriggeredByUserId);

        builder
            .HasOne(x => x.TriggeredByUser)
            .WithMany()
            .HasForeignKey(x => x.TriggeredByUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.NotifiedUser)
            .WithMany()
            .HasForeignKey(x => x.NotifiedUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
