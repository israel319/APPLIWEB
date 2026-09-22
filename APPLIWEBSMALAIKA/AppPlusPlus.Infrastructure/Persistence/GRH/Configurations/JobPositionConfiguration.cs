using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class JobPositionConfiguration : IEntityTypeConfiguration<JobPosition>
{
    public void Configure(EntityTypeBuilder<JobPosition> builder)
    {
        builder.ToTable("T_GRH_JobPositions");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.PositionCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.DepartmentId, x.PositionCode }).IsUnique();
        
        builder.Property(x => x.PositionName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Level).HasMaxLength(50);
        builder.Property(x => x.RequiredEducation).HasMaxLength(255);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        // Relations
        builder.HasOne(x => x.Department)
            .WithMany(d => d.JobPositions)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(x => x.Company)
            .WithMany(c => c.JobPositions)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(x => x.Employees)
            .WithOne(e => e.JobPosition)
            .HasForeignKey(e => e.JobPositionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
