using AppPlusPlus.Domain.Entities.GRH.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class TrainingBudgetConfiguration : IEntityTypeConfiguration<TrainingBudget>
{
    public void Configure(EntityTypeBuilder<TrainingBudget> builder)
    {
        builder.ToTable("T_GRH_TrainingBudgets");
        
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.DepartmentId, x.Year }).IsUnique();
        
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(x => x.Department)
            .WithMany(d => d.TrainingBudgets)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
