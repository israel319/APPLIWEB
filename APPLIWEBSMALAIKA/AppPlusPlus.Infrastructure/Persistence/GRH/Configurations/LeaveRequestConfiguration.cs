using AppPlusPlus.Domain.Entities.GRH.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("T_GRH_LeaveRequests");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.RejectionReason).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mappings (entity property → actual DB column)
        builder.Property(x => x.Status).HasColumnName("LeaveStatus").HasConversion<string>();
        builder.Property(x => x.ApprovedDate).HasColumnName("ApprovedAt");
        builder.Property(x => x.ApprovedById).HasColumnName("ApprovedBy");

        // Entity properties absent from DB schema
        builder.Ignore(x => x.RequestNumber);
        builder.Ignore(x => x.ReplacementPersonCode);
        builder.Ignore(x => x.Comments);
        builder.Ignore(x => x.IsRetroactive);
        builder.Ignore(x => x.IsPaid);
        builder.Ignore(x => x.UpdatedAt);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.LeaveRequests)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeaveType)
            .WithMany(lt => lt.LeaveRequests)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
