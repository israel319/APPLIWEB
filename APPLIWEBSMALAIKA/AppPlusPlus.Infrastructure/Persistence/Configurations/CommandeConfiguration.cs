using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Commandes;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class CommandeConfiguration : IEntityTypeConfiguration<Commande>
{
    public void Configure(EntityTypeBuilder<Commande> builder)
    {
        builder.ToTable("T_Commande", tb => tb.HasTrigger("TR_MonetaryNormalize_T_Commande"));

        // Commande -> CommandeDetails (1:N)
        builder.HasMany(c => c.Details)
            .WithOne(d => d.Commande)
            .HasForeignKey(d => d.CommandeId);

        // Commande -> Livraisons (1:N)
        builder.HasMany(c => c.Livraisons)
            .WithOne(l => l.Commande)
            .HasForeignKey(l => l.CommandeId);

        // Commande -> Factures (1:N via Fact.CommandeId, ref directe)
        builder.HasMany(c => c.Factures)
            .WithOne(f => f.Commande)
            .HasForeignKey(f => f.CommandeId);
    }
}
