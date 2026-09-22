using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class EmployeeClassificationConfiguration : IEntityTypeConfiguration<EmployeeClassification>
{
    public void Configure(EntityTypeBuilder<EmployeeClassification> builder)
    {
        builder.ToTable("T_GRH_EmployeeClassifications");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.ClassificationCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.ClassificationCode).IsUnique();
        
        builder.Property(x => x.ClassificationName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ContractType).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        // Relations
        builder.HasMany(x => x.Employees)
            .WithOne(e => e.EmployeeClassification)
            .HasForeignKey(e => e.EmployeeClassificationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
