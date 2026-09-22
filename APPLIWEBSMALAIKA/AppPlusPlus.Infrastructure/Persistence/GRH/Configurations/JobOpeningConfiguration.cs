using AppPlusPlus.Domain.Entities.GRH.Recruitment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class JobOpeningConfiguration : IEntityTypeConfiguration<JobOpening>
{
    public void Configure(EntityTypeBuilder<JobOpening> builder)
    {
        builder.ToTable("T_GRH_JobOpenings");

        builder.HasKey(x => x.Id);

        // Column name mappings (entity property → actual DB column)
        builder.Property(x => x.OpeningCode).HasColumnName("JobCode").HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.OpeningCode).IsUnique();

        builder.Property(x => x.Title).HasColumnName("JobTitle").HasMaxLength(255);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Entity properties absent from DB schema
        builder.Ignore(x => x.JobPositionId);
        builder.Ignore(x => x.JobPosition);
        builder.Ignore(x => x.EducationRequired);
        builder.Ignore(x => x.KeySkills);
        builder.Ignore(x => x.YearsOfExperienceRequired);
        builder.Ignore(x => x.IsActive);

        // Candidates relationship managed via T_GRH_CandidateApplications (no direct FK in DB)
        builder.Ignore(x => x.Candidates);
    }
}
