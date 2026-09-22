using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Demandes;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class DemandeConfiguration : IEntityTypeConfiguration<Demande>
{
    public void Configure(EntityTypeBuilder<Demande> builder)
    {
        builder.HasMany(d => d.Details)
            .WithOne(dd => dd.Demande)
            .HasForeignKey(dd => dd.IdDemande)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.LocalisationDemandeur)
            .WithMany()
            .HasForeignKey(d => d.IdLocalisationDemandeur)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.LocalisationSource)
            .WithMany()
            .HasForeignKey(d => d.IdLocalisationSource)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
