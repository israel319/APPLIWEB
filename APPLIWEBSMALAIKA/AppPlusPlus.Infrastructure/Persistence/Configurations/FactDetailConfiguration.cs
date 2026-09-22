using AppPlusPlus.Domain.Entities.Vente;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class FactDetailConfiguration : IEntityTypeConfiguration<FactDetail>
{
    public void Configure(EntityTypeBuilder<FactDetail> builder)
    {
        // Legacy trigger updates stock qty on insert/update/delete.
        // Required so EF Core avoids OUTPUT without INTO (SQL error 334).
        builder.ToTable("T_Fact_Details", tb =>
        {
            tb.HasTrigger("TRUpdateQte");
            tb.HasTrigger("TR_MonetaryNormalize_T_Fact_Details");
        });
    }
}
