using AppPlusPlus.Domain.Entities.GRH.Recruitment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("T_GRH_Candidates");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.CandidateCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.CandidateCode).IsUnique();

        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
        builder.Property(x => x.CurrentPosition).HasMaxLength(255);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mappings (entity property → actual DB column)
        builder.Property(x => x.PhoneNumber).HasColumnName("Phone").HasMaxLength(20);
        builder.Property(x => x.CurrentCompany).HasColumnName("CurrentEmployer").HasMaxLength(255);
        builder.Property(x => x.Resume).HasColumnName("ResumeFile").HasMaxLength(500);
        builder.Property(x => x.DateOfBirth).HasColumnName("BirthDate");

        // Entity properties absent from DB schema
        builder.Ignore(x => x.JobOpeningId);
        builder.Ignore(x => x.JobOpening);
        builder.Ignore(x => x.Status);
        builder.Ignore(x => x.Country);
        builder.Ignore(x => x.ApplicationDate);
        builder.Ignore(x => x.UpdatedAt);

        builder.HasMany(x => x.Interviews)
            .WithOne(i => i.Candidate)
            .HasForeignKey(i => i.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
