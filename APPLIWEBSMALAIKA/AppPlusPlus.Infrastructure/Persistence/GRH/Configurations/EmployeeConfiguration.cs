using AppPlusPlus.Domain.Entities.GRH.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("T_GRH_Employees");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.EmployeeCode).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Gender).HasMaxLength(1);
        builder.Property(x => x.MaritalStatus).HasMaxLength(50);
        builder.Property(x => x.Nationality).HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.ZipCode).HasMaxLength(20);
        builder.Property(x => x.BankName).HasMaxLength(255);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Column name mappings (entity property → actual DB column)
        builder.Property(x => x.PhoneNumber).HasColumnName("Phone").HasMaxLength(20);
        builder.Property(x => x.DateOfBirth).HasColumnName("BirthDate");
        builder.Property(x => x.PlaceOfBirth).HasColumnName("BirthPlace").HasMaxLength(100);
        builder.Property(x => x.NationalId).HasColumnName("IdNumber").HasMaxLength(50);
        builder.Property(x => x.EmployeeCategoryId).HasColumnName("CategoryId");
        builder.Property(x => x.EmployeeClassificationId).HasColumnName("ClassificationId");
        builder.Property(x => x.HireDate).HasColumnName("JoiningDate");
        builder.Property(x => x.Status).HasColumnName("EmployeeStatus").HasConversion<string>();
        builder.Property(x => x.ContractType).HasColumnName("EmploymentType").HasConversion<string>();
        builder.Property(x => x.AccountNumber).HasColumnName("BankAccountNumber").HasMaxLength(50);
        builder.Property(x => x.EmergencyContactName).HasColumnName("EmergencyContact").HasMaxLength(255);
        builder.Property(x => x.EmergencyContactPhone).HasColumnName("EmergencyPhone").HasMaxLength(50);

        // Entity properties absent from the DB schema
        builder.Ignore(x => x.MiddleName);
        builder.Ignore(x => x.PersonalEmail);
        builder.Ignore(x => x.MobileNumber);
        builder.Ignore(x => x.Country);
        builder.Ignore(x => x.ConfirmationDate);
        builder.Ignore(x => x.EndDate);
        builder.Ignore(x => x.ContractNumber);
        builder.Ignore(x => x.ContractStartDate);
        builder.Ignore(x => x.ContractEndDate);
        builder.Ignore(x => x.IBAN);
        builder.Ignore(x => x.BIC);
        builder.Ignore(x => x.EmergencyContactRelation);
        builder.Ignore(x => x.IsActive);
        builder.Ignore(x => x.Manager_EmployeeCode);

        // Relations
        builder.HasOne(x => x.Company)
            .WithMany(c => c.Employees)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Service)
            .WithMany(s => s.Employees)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.JobPosition)
            .WithMany(jp => jp.Employees)
            .HasForeignKey(x => x.JobPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EmployeeCategory)
            .WithMany(ec => ec.Employees)
            .HasForeignKey(x => x.EmployeeCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EmployeeClassification)
            .WithMany(ecl => ecl.Employees)
            .HasForeignKey(x => x.EmployeeClassificationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Contracts)
            .WithOne(c => c.Employee)
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Educations)
            .WithOne(e => e.Employee)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
