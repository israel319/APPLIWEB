using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

/// <summary>
/// Configuration pour l'entité Company
/// </summary>
public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("T_GRH_Companies");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.CompanyCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.CompanyCode).IsUnique();
        
        builder.Property(x => x.CompanyName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LegalName).HasMaxLength(255);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.RegistrationNumber).HasMaxLength(50);
        builder.Property(x => x.TaxId).HasMaxLength(50);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.ZipCode).HasMaxLength(20);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.Website).HasMaxLength(255);
        builder.Property(x => x.Logo).HasMaxLength(500);
        builder.Property(x => x.HeadquartersLocation).HasMaxLength(255);
        builder.Property(x => x.Currency).HasDefaultValue("TND");
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        // Relations
        builder.HasMany(x => x.Departments)
            .WithOne(d => d.Company)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(x => x.Services)
            .WithOne(s => s.Company)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(x => x.JobPositions)
            .WithOne(jp => jp.Company)
            .HasForeignKey(jp => jp.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(x => x.Employees)
            .WithOne(e => e.Company)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
