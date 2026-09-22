using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Finance;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class CaisseConfiguration : IEntityTypeConfiguration<Caisse>
{
    public void Configure(EntityTypeBuilder<Caisse> builder)
    {
        builder.HasKey(c => c.CaisseId);

        builder.Property(c => c.TotalRestDay)
            .HasComputedColumnSql();
    }
}
