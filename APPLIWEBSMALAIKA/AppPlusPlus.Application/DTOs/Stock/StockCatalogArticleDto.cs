namespace AppPlusPlus.Application.DTOs.Stock;

/// <summary>Vue catalogue article avec agrégats stock — calculés côté service, jamais en UI.</summary>
public class StockCatalogArticleDto
{
    public string IdArticle { get; set; } = "";
    public string Description { get; set; } = "";
    public string? DescriptionI { get; set; }
    public double? Price { get; set; }
    public bool IsUsd { get; set; }
    public int IdMarque { get; set; }
    public int IdCategory { get; set; }
    public string CategoryName { get; set; } = "";
    public int IdType { get; set; }
    public string TypeName { get; set; } = "";
    public bool IsOrderableOnline { get; set; }
    public int IdMesure { get; set; }
    public int LocationCount { get; set; }
    public decimal TotalStock { get; set; }
    public int TotalBudgetQty { get; set; }
    public int BudgetRestantQty { get; set; }
    public bool HasRupture { get; set; }
    public bool HasLowStock { get; set; }
}
