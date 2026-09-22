using AppPlusPlus.Domain.Entities.GRH.Absence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class AbsenceConfiguration : IEntityTypeConfiguration<Absence>
{
    public void Configure(EntityTypeBuilder<Absence> builder)
    {
        builder.ToTable("T_GRH_Absences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.Justification).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mappings
        builder.Property(x => x.Duration).HasColumnName("Hours");
        builder.Property(x => x.JustificationDocument).HasColumnName("Attachment").HasMaxLength(500);
        builder.Property(x => x.Status).HasColumnName("AbsenceStatus").HasMaxLength(50);

        // Entity properties absent from DB
        builder.Ignore(x => x.AbsenceNumber);
        builder.Ignore(x => x.IsPaid);
        builder.Ignore(x => x.UpdatedAt);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.Absences)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AbsenceType)
            .WithMany(at => at.Absences)
            .HasForeignKey(x => x.AbsenceTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
