namespace AppPlusPlus.Web.Services;

public class ClientCartLine
{
    public string ArticleId { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ImageArticle { get; set; }
    public int IdMonais { get; set; }
    public decimal Pu { get; set; }
    public int Qty { get; set; }
}
