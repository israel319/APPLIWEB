using AppPlusPlus.Domain.Entities.GRH.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class EmployeeEducationConfiguration : IEntityTypeConfiguration<EmployeeEducation>
{
    public void Configure(EntityTypeBuilder<EmployeeEducation> builder)
    {
        builder.ToTable("T_GRH_EmployeeEducations");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EducationLevel).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DegreeType).HasMaxLength(100);
        builder.Property(x => x.Institution).HasMaxLength(255);
        builder.Property(x => x.FieldOfStudy).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(x => x.Employee)
            .WithMany(e => e.Educations)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
