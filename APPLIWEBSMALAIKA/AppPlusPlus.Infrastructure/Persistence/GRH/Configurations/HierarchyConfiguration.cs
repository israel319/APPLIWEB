using AppPlusPlus.Domain.Entities.GRH.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class HierarchyConfiguration : IEntityTypeConfiguration<Hierarchy>
{
    public void Configure(EntityTypeBuilder<Hierarchy> builder)
    {
        builder.ToTable("T_GRH_Hierarchies");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.EmployeeId).IsRequired();
        builder.Property(x => x.SupervisedById).IsRequired();
        builder.Property(x => x.EffectiveDate).IsRequired();
    }
}
