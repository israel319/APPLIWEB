using AppPlusPlus.Domain.Entities.GRH.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class SalaryComponentConfiguration : IEntityTypeConfiguration<SalaryComponent>
{
    public void Configure(EntityTypeBuilder<SalaryComponent> builder)
    {
        builder.ToTable("T_GRH_SalaryComponents");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.ComponentCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.ComponentCode).IsUnique();

        builder.Property(x => x.ComponentName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ComponentType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CalculationMethod).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mapping: entity DefaultAmount → DB FixedAmount
        builder.Property(x => x.DefaultAmount).HasColumnName("FixedAmount");
    }
}
