using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class EmployeeCategoryConfiguration : IEntityTypeConfiguration<EmployeeCategory>
{
    public void Configure(EntityTypeBuilder<EmployeeCategory> builder)
    {
        builder.ToTable("T_GRH_EmployeeCategories");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.CategoryCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.CategoryCode).IsUnique();
        
        builder.Property(x => x.CategoryName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.BaseSalaryBand).HasMaxLength(50);
        builder.Property(x => x.HealthInsuranceType).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        // Relations
        builder.HasMany(x => x.Employees)
            .WithOne(e => e.EmployeeCategory)
            .HasForeignKey(e => e.EmployeeCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
