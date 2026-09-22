using AppPlusPlus.Domain.Entities.GRH.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("T_GRH_DocumentTypes");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentTypeCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.DocumentTypeCode).IsUnique();
        
        builder.Property(x => x.DocumentTypeName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasMany(x => x.EmployeeDocuments)
            .WithOne(ed => ed.DocumentType)
            .HasForeignKey(ed => ed.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
