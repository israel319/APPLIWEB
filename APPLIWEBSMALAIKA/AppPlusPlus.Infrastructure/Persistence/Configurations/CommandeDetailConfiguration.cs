using AppPlusPlus.Domain.Entities.Commandes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class CommandeDetailConfiguration : IEntityTypeConfiguration<CommandeDetail>
{
    public void Configure(EntityTypeBuilder<CommandeDetail> builder)
    {
        builder.ToTable("T_Commande_Details", tb => tb.HasTrigger("TR_MonetaryNormalize_T_Commande_Details"));
    }
}
