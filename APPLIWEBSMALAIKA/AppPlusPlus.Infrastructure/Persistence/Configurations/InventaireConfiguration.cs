using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Inventaire;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class InventaireMagasinConfiguration : IEntityTypeConfiguration<InventaireMagasin>
{
    public void Configure(EntityTypeBuilder<InventaireMagasin> builder)
    {
        builder.HasOne(x => x.Localisation)
            .WithMany()
            .HasForeignKey(x => x.IdLocalisation)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Receptions)
            .WithOne(x => x.Inventaire)
            .HasForeignKey(x => x.IdInventaire)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Ventes)
            .WithOne(x => x.Inventaire)
            .HasForeignKey(x => x.IdInventaire)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.MontantTheorique)
            .HasComputedColumnSql("(Montant_Initial + Montant_Receptions - Montant_Ventes)");
    }
}

public class InventaireReceptionConfiguration : IEntityTypeConfiguration<InventaireReception>
{
    public void Configure(EntityTypeBuilder<InventaireReception> builder)
    {
        builder.ToTable("T_Inventaire_Reception", tb => tb.HasTrigger("TR_T_Inventaire_Reception_RefreshSession"));

        builder.HasIndex(x => new { x.IdInventaire, x.DateJour }).IsUnique();

        builder.HasMany(x => x.Details)
            .WithOne(x => x.Reception)
            .HasForeignKey(x => x.IdReception)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InventaireReceptionDetailConfiguration : IEntityTypeConfiguration<InventaireReceptionDetail>
{
    public void Configure(EntityTypeBuilder<InventaireReceptionDetail> builder)
    {
        builder.ToTable("T_Inventaire_Reception_Detail", tb =>
            tb.HasTrigger("TR_T_Inventaire_Reception_Detail_RefreshTotal"));

        builder.HasIndex(x => new { x.IdReception, x.DateLigne });

        builder.HasOne(x => x.Article)
            .WithMany()
            .HasForeignKey(x => x.IdArticle)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.MontantLigne)
            .HasComputedColumnSql("(Quantite * Prix_Unitaire)");
    }
}

public class InventaireVenteConfiguration : IEntityTypeConfiguration<InventaireVente>
{
    public void Configure(EntityTypeBuilder<InventaireVente> builder)
    {
        builder.ToTable("T_Inventaire_Vente", tb => tb.HasTrigger("TR_T_Inventaire_Vente_RefreshSession"));

        builder.HasIndex(x => new { x.IdInventaire, x.DateVente }).IsUnique();
    }
}
