using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Threads.Api.Data.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // IsRequired() is unnecessary for required properties
        //  and also for value types
        // DateTime properties are not configured because
        //  there is no additional information about them

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.Bio).HasMaxLength(300);
        builder.Property(x => x.IsPrivate).HasDefaultValue(false);
    }
}
