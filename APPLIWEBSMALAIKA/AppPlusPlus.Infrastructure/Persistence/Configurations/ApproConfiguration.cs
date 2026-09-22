using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Approvisionnement;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class ApproConfiguration : IEntityTypeConfiguration<Appro>
{
    public void Configure(EntityTypeBuilder<Appro> builder)
    {
        builder.ToTable("T_Appros", tb =>
        {
            tb.HasTrigger("TRUpdateQte_");
            tb.HasTrigger("TR_MonetaryNormalize_T_Appros");
        });

        // Appro -> ApproDetails (1:N)
        builder.HasMany(a => a.Details)
            .WithOne(d => d.Appro)
            .HasForeignKey(d => d.IdAppro);
    }
}
