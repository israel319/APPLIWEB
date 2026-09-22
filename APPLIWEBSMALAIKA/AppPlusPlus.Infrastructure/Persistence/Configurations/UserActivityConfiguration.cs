using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
{
    public void Configure(EntityTypeBuilder<UserActivity> builder)
    {
        // Unique index (UserLogin, ActivityId)
        builder.HasIndex(ua => new { ua.UserLogin, ua.ActivityId })
            .IsUnique();
    }
}
