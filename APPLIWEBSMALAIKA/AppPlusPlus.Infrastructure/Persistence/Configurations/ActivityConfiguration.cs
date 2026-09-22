using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        // Activity -> UserActivities (1:N)
        builder.HasMany(a => a.UserActivities)
            .WithOne(ua => ua.Activity)
            .HasForeignKey(ua => ua.ActivityId);

        // Unique indexes
        builder.HasIndex(a => a.Code)
            .IsUnique();

        builder.HasIndex(a => new { a.FonctionId, a.DescriptionActivity })
            .IsUnique();
    }
}
