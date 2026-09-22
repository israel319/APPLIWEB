using AppPlusPlus.Domain.Entities.Parametres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class InvoiceTemplateConfiguration : IEntityTypeConfiguration<InvoiceTemplate>
{
    public void Configure(EntityTypeBuilder<InvoiceTemplate> builder)
    {
        builder.Property(t => t.Nom).HasMaxLength(120).IsRequired();
        builder.Property(t => t.PrintFormat).HasMaxLength(20).IsRequired();
        builder.HasIndex(t => t.IsActive);
    }
}
