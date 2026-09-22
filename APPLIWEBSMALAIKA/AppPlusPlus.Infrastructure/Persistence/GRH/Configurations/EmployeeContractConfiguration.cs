using AppPlusPlus.Domain.Entities.GRH.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class EmployeeContractConfiguration : IEntityTypeConfiguration<EmployeeContract>
{
    public void Configure(EntityTypeBuilder<EmployeeContract> builder)
    {
        builder.ToTable("T_GRH_EmployeeContracts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.ContractNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ContractType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mapping
        builder.Property(x => x.DocumentPath).HasColumnName("ContractPdf").HasMaxLength(500);

        // Entity properties absent from DB
        builder.Ignore(x => x.ConfirmationDate);
        builder.Ignore(x => x.Terms);
        builder.Ignore(x => x.IsActive);
        builder.Ignore(x => x.UpdatedAt);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.Contracts)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
