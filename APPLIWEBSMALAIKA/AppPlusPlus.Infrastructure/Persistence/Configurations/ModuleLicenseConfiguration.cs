using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class ModuleLicenseConfiguration : IEntityTypeConfiguration<ModuleLicense>
{
    public void Configure(EntityTypeBuilder<ModuleLicense> builder)
    {
        builder.ToTable("T_ModuleLicenses");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ModuleCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(m => m.ModuleCode)
            .IsUnique();

        builder.Property(m => m.ModuleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Description)
            .HasMaxLength(500);

        builder.Property(m => m.Icon)
            .HasMaxLength(50);

        builder.Property(m => m.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(m => m.LicenseNotes)
            .HasMaxLength(500);

        builder.Property(m => m.CreatedAt)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(m => m.UpdatedAt)
            .HasDefaultValueSql("GETDATE()");
    }
}
