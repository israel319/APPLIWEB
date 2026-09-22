using AppPlusPlus.Domain.Entities.Approvisionnement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class ApproExpenseConfiguration : IEntityTypeConfiguration<ApproExpense>
{
    public void Configure(EntityTypeBuilder<ApproExpense> builder)
    {
        builder.ToTable("T_Appro_Expense", tb => tb.HasTrigger("TR_MonetaryNormalize_T_Appro_Expense"));
    }
}
