using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Prestations;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class ServiceCatalogItemConfiguration : IEntityTypeConfiguration<ServiceCatalogItem>
{
    public void Configure(EntityTypeBuilder<ServiceCatalogItem> builder)
    {
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Price).HasPrecision(18, 2);

        builder.HasOne(x => x.Money)
            .WithMany()
            .HasForeignKey(x => x.MoneyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
