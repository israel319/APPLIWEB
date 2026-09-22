using AppPlusPlus.Domain.Entities.GRH.Evaluation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppPlusPlus.Infrastructure.Persistence.GRH.Configurations;

public class EmployeeReviewConfiguration : IEntityTypeConfiguration<EmployeeReview>
{
    public void Configure(EntityTypeBuilder<EmployeeReview> builder)
    {
        builder.ToTable("T_GRH_EmployeeReviews");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Period).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ReviewType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ReviewText).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
