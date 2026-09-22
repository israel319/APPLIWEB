using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Stock;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class MouvementStockConfiguration : IEntityTypeConfiguration<MouvementStock>
{
    public void Configure(EntityTypeBuilder<MouvementStock> builder)
    {
        builder.HasKey(m => m.Id);

        // MouvementStock : Colonne calculée
        builder.Property(m => m.ValeurTotale)
            .HasComputedColumnSql();

        // MouvementStock : Index pour recherche
        builder.HasIndex(m => m.IdArticle);

        builder.HasIndex(m => m.IdLocalisation);

        builder.HasIndex(m => m.DateMouvement);

        builder.HasIndex(m => new { m.TypeDocument, m.IdDocument });
    }
}
