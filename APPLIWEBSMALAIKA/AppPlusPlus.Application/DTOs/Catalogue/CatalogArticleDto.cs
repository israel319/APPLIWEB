namespace AppPlusPlus.Application.DTOs.Catalogue;

public class CatalogArticleDto
{
    public string IdArticle { get; set; } = "";
    public string? Description { get; set; }
    public string? ImageArticle { get; set; }
    public int IdMonais { get; set; }
    public double Price { get; set; }
    public int IdCategory { get; set; }
    public string? CategoryName { get; set; }
    public List<CatalogLocalisationDto> Localisations { get; set; } = new();
}

public class CatalogLocalisationDto
{
    public string Name { get; set; } = "";
    public decimal Qte { get; set; }
}
