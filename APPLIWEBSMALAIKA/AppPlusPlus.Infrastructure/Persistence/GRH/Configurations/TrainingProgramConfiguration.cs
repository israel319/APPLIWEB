using AppPlusPlus.Domain.Entities.GRH.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class TrainingProgramConfiguration : IEntityTypeConfiguration<TrainingProgram>
{
    public void Configure(EntityTypeBuilder<TrainingProgram> builder)
    {
        builder.ToTable("T_GRH_TrainingPrograms");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProgramCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.ProgramCode).IsUnique();

        builder.Property(x => x.ProgramName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Cost).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mappings (entity property → actual DB column)
        builder.Property(x => x.Category).HasColumnName("TrainingType").HasMaxLength(100);
        builder.Property(x => x.DurationHours).HasColumnName("Duration");
        builder.Property(x => x.Provider).HasColumnName("TrainingProvider").HasMaxLength(255);

        // Entity properties absent from DB schema
        builder.Ignore(x => x.IsActive);
        builder.Ignore(x => x.IsExternal);

        builder.HasMany(x => x.Sessions)
            .WithOne(ts => ts.TrainingProgram)
            .HasForeignKey(ts => ts.TrainingProgramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
