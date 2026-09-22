using AppPlusPlus.Domain.Entities.GRH.Absence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class AbsenceTypeConfiguration : IEntityTypeConfiguration<AbsenceType>
{
    public void Configure(EntityTypeBuilder<AbsenceType> builder)
    {
        builder.ToTable("T_GRH_AbsenceTypes");

        builder.HasKey(x => x.Id);

        // Column name mappings (entity → DB)
        builder.Property(x => x.AbsenceTypeCode)
               .HasColumnName("AbsenceCode")
               .HasMaxLength(50)
               .IsRequired();
        builder.HasIndex(x => x.AbsenceTypeCode).IsUnique();

        builder.Property(x => x.AbsenceTypeName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        // Entity properties absent from the DB schema
        builder.Ignore(x => x.RequiresJustification);
        builder.Ignore(x => x.CountsAgainstLeave);
        builder.Ignore(x => x.CreatedAt);
        builder.Ignore(x => x.UpdatedAt);

        builder.HasMany(x => x.Absences)
               .WithOne(a => a.AbsenceType)
               .HasForeignKey(a => a.AbsenceTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
