using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("T_GRH_Services");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.ServiceCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.DepartmentId, x.ServiceCode }).IsUnique();
        
        builder.Property(x => x.ServiceName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Location).HasMaxLength(255);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        // Relations
        builder.HasOne(x => x.Department)
            .WithMany(d => d.Services)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(x => x.Company)
            .WithMany(c => c.Services)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(x => x.Employees)
            .WithOne(e => e.Service)
            .HasForeignKey(e => e.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // T_GRH_JobPositions has no ServiceId column — positions belong to departments, not services
        builder.Ignore(x => x.JobPositions);
    }
}
