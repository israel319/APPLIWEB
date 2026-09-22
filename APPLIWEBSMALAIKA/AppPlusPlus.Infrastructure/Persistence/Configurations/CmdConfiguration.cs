using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.CommandesInternes;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class CmdConfiguration : IEntityTypeConfiguration<Cmd>
{
    public void Configure(EntityTypeBuilder<Cmd> builder)
    {
        // Cmd -> CmdDetails (1:N)
        builder.HasMany(c => c.Details)
            .WithOne(d => d.Cmd)
            .HasForeignKey(d => d.IdCmd);
    }
}
