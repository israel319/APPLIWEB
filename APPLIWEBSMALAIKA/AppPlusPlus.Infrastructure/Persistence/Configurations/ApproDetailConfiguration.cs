using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Approvisionnement;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class ApproDetailConfiguration : IEntityTypeConfiguration<ApproDetail>
{
    public void Configure(EntityTypeBuilder<ApproDetail> builder)
    {
        builder.ToTable("T_Appro_Details", tb => tb.HasTrigger("TR_MonetaryNormalize_T_Appro_Details"));

        builder.HasKey(a => a.Id);
    }
}
