using AppPlusPlus.Domain.Entities.GRH.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.ToTable("T_GRH_PayrollRuns");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.PayrollNumber).HasColumnName("PayrollCode").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.PayrollNumber).IsUnique();

        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mappings
        builder.Property(x => x.PayPeriodStart).HasColumnName("PeriodStartDate");
        builder.Property(x => x.PayPeriodEnd).HasColumnName("PeriodEndDate");
        builder.Property(x => x.Status).HasColumnName("PayrollStatus").HasConversion<string>();
        builder.Property(x => x.PaidDate).HasColumnName("PaidAt");

        // Entity properties absent from DB
        builder.Ignore(x => x.Year);
        builder.Ignore(x => x.Month);
        builder.Ignore(x => x.ProcessedDate);
        builder.Ignore(x => x.PaymentMethod);
        builder.Ignore(x => x.UpdatedAt);

        builder.HasMany(x => x.PayrollDetails)
            .WithOne(pd => pd.PayrollRun)
            .HasForeignKey(pd => pd.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
