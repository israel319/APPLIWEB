using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class LocalisationConfiguration : IEntityTypeConfiguration<Localisation>
{
    public void Configure(EntityTypeBuilder<Localisation> builder)
    {
        // Localisation -> UserLocalisations (1:N) — une localisation peut avoir plusieurs utilisateurs
        builder.HasMany(l => l.UserLocalisations)
            .WithOne(ul => ul.Localisation)
            .HasForeignKey(ul => ul.LocalisationId);
    }
}
