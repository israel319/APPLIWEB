using AppPlusPlus.Domain.Entities.Commandes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class LivraisonDetailConfiguration : IEntityTypeConfiguration<LivraisonDetail>
{
    public void Configure(EntityTypeBuilder<LivraisonDetail> builder)
    {
        builder.ToTable("T_Livraison_Detail", tb => tb.HasTrigger("TR_MonetaryNormalize_T_Livraison_Detail"));
    }
}
