using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppPlusPlus.Domain.Entities.Stock;

namespace AppPlusPlus.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        // Aligné sur PK_T_Art_Localisations en base : (Id_Article, Id_Localisation).
        builder.HasKey(s => new { s.IdArticle, s.IdLocalisation });
        builder.Property(s => s.Id).ValueGeneratedOnAdd();
    }
}
