using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("T_GRH_Departments");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.DepartmentCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.DepartmentCode }).IsUnique();
        
        builder.Property(x => x.DepartmentName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Location).HasMaxLength(255);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        // Relations
        builder.HasOne(x => x.Company)
            .WithMany(c => c.Departments)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(x => x.Services)
            .WithOne(s => s.Department)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(x => x.JobPositions)
            .WithOne(jp => jp.Department)
            .HasForeignKey(jp => jp.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(x => x.Employees)
            .WithOne(e => e.Department)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(x => x.TrainingBudgets)
            .WithOne(tb => tb.Department)
            .HasForeignKey(tb => tb.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
