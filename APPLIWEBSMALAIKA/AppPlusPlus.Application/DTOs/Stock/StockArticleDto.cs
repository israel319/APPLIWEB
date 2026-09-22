namespace AppPlusPlus.Application.DTOs.Stock;

public class StockArticleDto
{
    public string IdArticle { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Qte { get; set; }
    public decimal Seuil { get; set; }
    public int QteMax { get; set; }
    public double Price { get; set; }
}
