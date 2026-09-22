using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Finance;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class VersementConfiguration : IEntityTypeConfiguration<Versement>
{
    public void Configure(EntityTypeBuilder<Versement> builder)
    {
        builder.ToTable("T_Versements", tb => tb.HasTrigger("TR_MonetaryNormalize_T_Versements"));

        builder.HasKey(v => v.Id);

        builder.HasIndex(v => new { v.DateCloture, v.LocalisationId })
            .IsUnique();
    }
}
