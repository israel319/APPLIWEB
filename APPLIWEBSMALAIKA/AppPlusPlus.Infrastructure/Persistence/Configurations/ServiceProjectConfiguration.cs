using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Prestations;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class ServiceProjectConfiguration : IEntityTypeConfiguration<ServiceProject>
{
    public void Configure(EntityTypeBuilder<ServiceProject> builder)
    {
        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Timesheets)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Deliverables)
            .WithOne(d => d.Project)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Facts)
            .WithOne(f => f.ServiceProject)
            .HasForeignKey(f => f.ServiceProjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ServiceTaskConfiguration : IEntityTypeConfiguration<ServiceTask>
{
    public void Configure(EntityTypeBuilder<ServiceTask> builder)
    {
        builder.HasMany(t => t.Timesheets)
            .WithOne(ts => ts.Task)
            .HasForeignKey(ts => ts.TaskId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
