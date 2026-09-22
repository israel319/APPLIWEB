namespace AppPlusPlus.Web.Components.Features.Stock;

public class StockLocationInfo
{
    public int StockId { get; set; }
    public int IdLocalisation { get; set; }
    public string LocalisationName { get; set; } = "";
    public decimal Qte { get; set; }
    public int Seuil { get; set; }
    public int QteMax { get; set; }

    /// <summary>Quantité restante pour atteindre le budget (qté max − stock).</summary>
    public decimal ResteBudget => Math.Max(0, QteMax - Qte);
}
