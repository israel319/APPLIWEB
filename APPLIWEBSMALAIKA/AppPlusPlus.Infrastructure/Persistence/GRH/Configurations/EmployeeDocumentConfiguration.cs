using AppPlusPlus.Domain.Entities.GRH.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("T_GRH_EmployeeDocuments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mappings (entity property → actual DB column)
        builder.Property(x => x.DocumentNumber).HasColumnName("DocumentName").HasMaxLength(255).IsRequired();
        builder.Property(x => x.DocumentPath).HasColumnName("DocumentFile").HasMaxLength(500);

        // Entity properties absent from DB schema
        builder.Ignore(x => x.Description);
        builder.Ignore(x => x.IsExpiringSoon);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.Documents)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.DocumentType)
            .WithMany(dt => dt.EmployeeDocuments)
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
