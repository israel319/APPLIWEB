using AppPlusPlus.Domain.Entities.GRH.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class EmployeeSkillConfiguration : IEntityTypeConfiguration<EmployeeSkill>
{
    public void Configure(EntityTypeBuilder<EmployeeSkill> builder)
    {
        builder.ToTable("T_GRH_EmployeeSkills");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SkillName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(50);
        builder.Property(x => x.Certification).HasMaxLength(255);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(x => x.Employee)
            .WithMany(e => e.Skills)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
