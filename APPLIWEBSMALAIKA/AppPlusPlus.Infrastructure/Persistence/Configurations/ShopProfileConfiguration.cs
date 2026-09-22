using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Parametres;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class ShopProfileConfiguration : IEntityTypeConfiguration<ShopProfile>
{
    public void Configure(EntityTypeBuilder<ShopProfile> builder)
    {
        // ShopProfile -> AppSetting (N:1)
        builder.HasOne(p => p.AppNameSetting)
            .WithMany(a => a.ShopProfiles)
            .HasForeignKey(p => p.AppNameSettingKey)
            .HasPrincipalKey(a => a.Key)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
