using AppPlusPlus.Domain.Entities.GRH.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentHistory>
{
    public void Configure(EntityTypeBuilder<PaymentHistory> builder)
    {
        builder.ToTable("T_GRH_PaymentHistories");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PaymentMethod).HasMaxLength(50);
        builder.Property(x => x.Reference).HasMaxLength(100);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(x => x.Employee)
            .WithMany(e => e.PaymentHistories)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(x => x.PayrollRun)
            .WithMany()
            .HasForeignKey(x => x.PayrollRunId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
