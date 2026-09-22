namespace AppPlusPlus.Application.DTOs.Dashboard;

public class DashboardDataDto
{
    // KPIs
    public decimal VentesToday { get; set; }
    public decimal VentesTodayFc { get; set; }
    public int FactsTodayCount { get; set; }
    public decimal DisplayTaux { get; set; }
    public decimal TotalApprosMonth { get; set; }
    public int ApprosMonthCount { get; set; }
    public decimal Benefice { get; set; }
    public decimal VentesMois { get; set; }
    public string TrendVente { get; set; } = "";
    public bool TrendVenteUp { get; set; }
    public decimal TotalRevenueYear { get; set; }
    public int FactsMonthCount { get; set; }
    public int CmdsEnAttente { get; set; }

    // Chart data
    public List<MonthlyDataDto> RevenueData { get; set; } = new();
    public List<StatusDataDto> StatusData { get; set; } = new();

    // Lists
    public List<TopArticleDto> TopArticles { get; set; } = new();
    public List<StockAlertDto> LowStockArticles { get; set; } = new();
    public List<RecentFactDto> RecentFacts { get; set; } = new();

    // Quick stats
    public int TotalClients { get; set; }
    public int ClientsPermanents { get; set; }
    public int TotalArticles { get; set; }
    public int TotalCommandes { get; set; }
}

public class MonthlyDataDto
{
    public string Month { get; set; } = "";
    public decimal Ventes { get; set; }
    public decimal Appros { get; set; }
}

public class StatusDataDto
{
    public string Label { get; set; } = "";
    public int Count { get; set; }
}

public class TopArticleDto
{
    public int Rank { get; set; }
    public string Description { get; set; } = "";
    public double QteVendue { get; set; }
    public double Montant { get; set; }
}

public class StockAlertDto
{
    public string Description { get; set; } = "";
    public decimal Qte { get; set; }
    public int Seuil { get; set; }
    public int QteMax { get; set; }
    public double Pct { get; set; }
}

public class RecentFactDto
{
    public int Id { get; set; }
    public string Client { get; set; } = "";
    public DateOnly Date { get; set; }
    public decimal Total { get; set; }
    public string StatusLabel { get; set; } = "";
}
