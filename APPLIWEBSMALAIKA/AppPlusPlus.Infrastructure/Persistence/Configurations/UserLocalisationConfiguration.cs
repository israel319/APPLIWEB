using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class UserLocalisationConfiguration : IEntityTypeConfiguration<UserLocalisation>
{
    public void Configure(EntityTypeBuilder<UserLocalisation> builder)
    {
        builder.HasKey(u => u.Id);
    }
}
