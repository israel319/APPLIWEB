using AppPlusPlus.Domain.Entities.GRH.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class SalaryConfiguration : IEntityTypeConfiguration<Salary>
{
    public void Configure(EntityTypeBuilder<Salary> builder)
    {
        builder.ToTable("T_GRH_Salaries");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.PaymentFrequency).HasMaxLength(50);
        builder.Property(x => x.SalaryBand).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // T_GRH_Salaries has no SalaryComponentId column — ignore this FK
        builder.Ignore(x => x.SalaryComponentId);
        builder.Ignore(x => x.SalaryComponent);
        builder.Ignore(x => x.IsActive);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.Salaries)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
