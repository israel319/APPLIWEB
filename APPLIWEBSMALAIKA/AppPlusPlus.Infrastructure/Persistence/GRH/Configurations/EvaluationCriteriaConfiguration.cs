using AppPlusPlus.Domain.Entities.GRH.Evaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class EvaluationCriteriaConfiguration : IEntityTypeConfiguration<EvaluationCriteria>
{
    public void Configure(EntityTypeBuilder<EvaluationCriteria> builder)
    {
        builder.ToTable("T_GRH_EvaluationCriteria");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CriteriaName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Comment).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(x => x.PerformanceEvaluation)
            .WithMany(pe => pe.Criteria)
            .HasForeignKey(x => x.PerformanceEvaluationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
