using AppPlusPlus.Domain.Entities.Vente;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // Trigger monétaire (Migration_Monetary_Rounding_Enforcement).
        // Required so EF Core avoids OUTPUT without INTO (SQL error 334).
        builder.ToTable("T_Payments", tb => tb.HasTrigger("TR_MonetaryNormalize_T_Payments"));
    }
}
