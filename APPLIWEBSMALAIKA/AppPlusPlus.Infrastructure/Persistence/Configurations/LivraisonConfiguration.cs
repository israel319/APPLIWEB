using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Commandes;
using AppPlusPlus.Domain.Entities.Vente;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class LivraisonConfiguration : IEntityTypeConfiguration<Livraison>
{
    public void Configure(EntityTypeBuilder<Livraison> builder)
    {
        // Livraison -> LivraisonDetails (1:N)
        builder.HasMany(l => l.Details)
            .WithOne(d => d.Livraison)
            .HasForeignKey(d => d.LivraisonId);

        // Livraison -> Fact (1:0..1 — au paiement)
        builder.HasOne(l => l.Fact)
            .WithOne(f => f.Livraison)
            .HasForeignKey<Fact>(f => f.LivraisonId);
    }
}
