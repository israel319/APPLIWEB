using AppPlusPlus.Domain.Entities.GRH.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class EmployeeTrainingConfiguration : IEntityTypeConfiguration<EmployeeTraining>
{
    public void Configure(EntityTypeBuilder<EmployeeTraining> builder)
    {
        builder.ToTable("T_GRH_EmployeeTrainings");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.Feedback).HasMaxLength(1000);
        builder.Property(x => x.Score).HasColumnType("decimal(5,2)");
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mappings (entity property → actual DB column)
        builder.Property(x => x.Certified).HasColumnName("CertificateIssued");

        // Entity properties absent from DB schema
        builder.Ignore(x => x.Attended);
        builder.Ignore(x => x.CertificateDate);
        builder.Ignore(x => x.Rating);
        builder.Ignore(x => x.UpdatedAt);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.Trainings)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TrainingSession)
            .WithMany(ts => ts.EmployeeTrainings)
            .HasForeignKey(x => x.TrainingSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
