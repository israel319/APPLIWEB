using AppPlusPlus.Domain.Entities.GRH.Evaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class PerformanceEvaluationConfiguration : IEntityTypeConfiguration<PerformanceEvaluation>
{
    public void Configure(EntityTypeBuilder<PerformanceEvaluation> builder)
    {
        builder.ToTable("T_GRH_PerformanceEvaluations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.EvaluationCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.EvaluationCode).IsUnique();

        builder.Property(x => x.Comments).HasMaxLength(2000);
        builder.Property(x => x.Strengths).HasMaxLength(1000);
        builder.Property(x => x.AreasForImprovement).HasMaxLength(1000);
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mappings (entity property → actual DB column)
        builder.Property(x => x.Period).HasColumnName("EvaluationPeriod").HasMaxLength(50).IsRequired();
        builder.Property(x => x.EvaluatedById).HasColumnName("EvaluatorId");
        builder.Property(x => x.OverallRating).HasColumnName("OverallScore").HasColumnType("decimal(5,2)");

        // Entity properties absent from DB schema
        builder.Ignore(x => x.GoalsForNextPeriod);
        builder.Ignore(x => x.UpdatedAt);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.Evaluations)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Criteria)
            .WithOne(ec => ec.PerformanceEvaluation)
            .HasForeignKey(ec => ec.PerformanceEvaluationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
