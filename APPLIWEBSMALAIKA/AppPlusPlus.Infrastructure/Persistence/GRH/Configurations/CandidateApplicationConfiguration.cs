using AppPlusPlus.Domain.Entities.GRH.Recruitment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class CandidateApplicationConfiguration : IEntityTypeConfiguration<CandidateApplication>
{
    public void Configure(EntityTypeBuilder<CandidateApplication> builder)
    {
        builder.ToTable("T_GRH_CandidateApplications");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ApplicationNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.ApplicationNumber).IsUnique();
        
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(x => x.Candidate)
            .WithMany(c => c.Applications)
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
