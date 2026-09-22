using AppPlusPlus.Domain.Entities.GRH.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("T_GRH_LeaveTypes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.LeaveTypeCode).HasColumnName("LeaveCode").HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.LeaveTypeCode).IsUnique();

        builder.Property(x => x.LeaveTypeName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mapping
        builder.Property(x => x.DefaultDays).HasColumnName("DaysAllowedPerYear");

        // Entity property absent from DB
        builder.Ignore(x => x.IsRecurring);

        builder.HasMany(x => x.LeaveRequests)
            .WithOne(lr => lr.LeaveType)
            .HasForeignKey(lr => lr.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.LeaveSaldos)
            .WithOne(ls => ls.LeaveType)
            .HasForeignKey(ls => ls.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
