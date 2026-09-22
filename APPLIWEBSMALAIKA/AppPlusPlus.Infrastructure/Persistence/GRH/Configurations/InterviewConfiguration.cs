using AppPlusPlus.Domain.Entities.GRH.Recruitment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.ToTable("T_GRH_Interviews");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.InterviewType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Feedback).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mappings (entity property → actual DB column)
        builder.Property(x => x.Status).HasColumnName("Result").HasMaxLength(50);
        builder.Property(x => x.NextStep).HasColumnName("NextSteps").HasMaxLength(500);

        // Entity properties absent from DB schema
        builder.Ignore(x => x.InterviewCode);
        builder.Ignore(x => x.InterviewerName);
        builder.Ignore(x => x.DurationMinutes);
        builder.Ignore(x => x.RecommendedForHiring);
        builder.Ignore(x => x.UpdatedAt);

        builder.HasOne(x => x.Candidate)
            .WithMany(c => c.Interviews)
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
