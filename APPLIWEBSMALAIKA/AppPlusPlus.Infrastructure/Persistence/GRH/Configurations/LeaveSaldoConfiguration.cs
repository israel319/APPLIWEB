using AppPlusPlus.Domain.Entities.GRH.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class LeaveSaldoConfiguration : IEntityTypeConfiguration<LeaveSaldo>
{
    public void Configure(EntityTypeBuilder<LeaveSaldo> builder)
    {
        builder.ToTable("T_GRH_LeaveSaldos");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.EmployeeId, x.LeaveTypeId, x.Year }).IsUnique();

        // Column name mappings
        builder.Property(x => x.TotalEntitledDays).HasColumnName("AllowedDays");
        builder.Property(x => x.LastResetDate).HasColumnName("LastUpdated");

        // Entity properties absent from DB
        builder.Ignore(x => x.RemainingDays);
        builder.Ignore(x => x.CarriedOverExpiringDays);
        builder.Ignore(x => x.HasExceededBalance);
        builder.Ignore(x => x.CreatedAt);
        builder.Ignore(x => x.UpdatedAt);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.LeaveSaldos)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeaveType)
            .WithMany(lt => lt.LeaveSaldos)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
