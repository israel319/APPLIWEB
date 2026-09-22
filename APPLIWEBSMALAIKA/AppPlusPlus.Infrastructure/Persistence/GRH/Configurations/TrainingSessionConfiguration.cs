using AppPlusPlus.Domain.Entities.GRH.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
{
    public void Configure(EntityTypeBuilder<TrainingSession> builder)
    {
        builder.ToTable("T_GRH_TrainingSessions");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SessionCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.SessionCode).IsUnique();
        
        builder.Property(x => x.Location).HasMaxLength(255);
        builder.Property(x => x.Trainer).HasMaxLength(255);
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(x => x.TrainingProgram)
            .WithMany(tp => tp.Sessions)
            .HasForeignKey(x => x.TrainingProgramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
