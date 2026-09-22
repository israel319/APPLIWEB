using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Catalogue;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        // Article : colonne calculée TotalPrice
        builder.Property(a => a.TotalPrice)
            .HasComputedColumnSql();

        builder.Property(a => a.ImageArticle)
            .HasColumnName("Image_Article")
            .HasColumnType("nvarchar(max)");
    }
}
