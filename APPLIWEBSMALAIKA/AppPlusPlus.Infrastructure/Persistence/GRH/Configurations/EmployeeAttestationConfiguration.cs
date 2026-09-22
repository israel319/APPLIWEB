using AppPlusPlus.Domain.Entities.GRH.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class EmployeeAttestationConfiguration : IEntityTypeConfiguration<EmployeeAttestation>
{
    public void Configure(EntityTypeBuilder<EmployeeAttestation> builder)
    {
        builder.ToTable("T_GRH_EmployeeAttestations");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AttestationNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.AttestationNumber).IsUnique();
        
        builder.Property(x => x.AttestationType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IssuedBy).HasMaxLength(255);
        builder.Property(x => x.Signature).HasMaxLength(500);
        builder.Property(x => x.AttestationDocument).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(x => x.Employee)
            .WithMany(e => e.Attestations)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
