using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class FonctionConfiguration : IEntityTypeConfiguration<Fonction>
{
    public void Configure(EntityTypeBuilder<Fonction> builder)
    {
        // Fonction -> Permissions (1:N)
        builder.HasMany(f => f.Permissions)
            .WithOne(p => p.Fonction)
            .HasForeignKey(p => p.FonctionId);

        // Fonction -> Activities (1:N)
        builder.HasMany(f => f.Activities)
            .WithOne(a => a.Fonction)
            .HasForeignKey(a => a.FonctionId);
    }
}
