using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // User -> UserLocalisations (1:N) — un utilisateur peut avoir plusieurs localisations
        builder.HasMany(u => u.UserLocalisations)
            .WithOne(ul => ul.User)
            .HasForeignKey(ul => ul.UserId)
            .HasPrincipalKey(u => u.Login);

        // User -> UserActivities (1:N)
        builder.HasMany(u => u.UserActivities)
            .WithOne(ua => ua.User)
            .HasForeignKey(ua => ua.UserLogin)
            .HasPrincipalKey(u => u.Login);
    }
}
