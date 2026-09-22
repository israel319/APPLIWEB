using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.DTOs.Dashboard;
using AppPlusPlus.Application.Services.Dashboard;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.QueryServices;

public class DashboardService : IDashboardService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public DashboardService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    private static bool IsCountedSale(Fact f) => f.Status is 1 or 2;

    public async Task<DashboardDataDto> GetDashboardDataAsync()
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var today = DateTime.Today;
        var year = today.Year;
        var month = today.Month;

        var firstOfYear = new DateOnly(year, 1, 1);
        var lastOfYear = new DateOnly(year, 12, 31);
        var todayDate = DateOnly.FromDateTime(today);

        var latestTaux = await ctx.TauxChanges.OrderByDescending(t => t.Id).Select(t => t.TauxValue).FirstOrDefaultAsync();

        decimal FactTotalFc(Fact f) =>
            CurrencyFormat.FactStoredTotalToFc(f.TotalApresReduction, f.Total, f.Taux, latestTaux);

        // --- Factures annee & mois ---
        var factsYear = await ctx.Facts
            .Where(f => f.Date >= firstOfYear && f.Date <= lastOfYear)
            .ToListAsync();

        var salesYear = factsYear.Where(IsCountedSale).ToList();
        var factsMonth = factsYear.Where(f => f.Date.Month == month).ToList();
        var salesMonth = salesYear.Where(f => f.Date.Month == month).ToList();
        var factsToday = factsYear.Where(f => f.Date == todayDate).ToList();
        var salesToday = salesYear.Where(f => f.Date == todayDate).ToList();
        var salesPrevMonth = salesYear.Where(f => f.Date.Month == month - 1).ToList();

        var venteMois = salesMonth.Sum(FactTotalFc);
        var ventePrevMois = salesPrevMonth.Sum(FactTotalFc);
        var venteToday = salesToday.Sum(FactTotalFc);
        var venteTodayFc = venteToday;
        var factsMonthCount = factsMonth.Count(f => f.Status != 3);

        // --- Appros annee & mois (montants en FC) ---
        var approsYear = await ctx.Appros
            .Where(a => a.Date >= firstOfYear && a.Date <= lastOfYear)
            .ToListAsync();
        var approsMois = ApproFilters.CountedPurchases(approsYear.Where(a => a.Date.Month == month)).ToList();

        var totalAppros = ApproFilters.TotalPurchaseFc(approsMois);

        // --- Commandes en attente ---
        var cmdsEnAttente = await ctx.Cmds.CountAsync(c => c.Status != null && c.Status == 0);

        // --- Marge appros du mois (BenTotal en FC) ---
        var benefice = ApproFilters.TotalBenefitFc(approsMois);

        // --- Tendances ---
        var trendVente = ventePrevMois > 0
            ? ((venteMois - ventePrevMois) / ventePrevMois * 100).ToString("+0;-0") + "%"
            : "";
        var trendVenteUp = venteMois >= ventePrevMois;

        // --- Graphique revenus + appros mensuels ---
        var monthNames = new[] { "Jan", "Fev", "Mar", "Avr", "Mai", "Jun",
                                 "Jul", "Aou", "Sep", "Oct", "Nov", "Dec" };
        var revenueData = Enumerable.Range(1, 12)
            .Select(m => new MonthlyDataDto
            {
                Month = monthNames[m - 1],
                Ventes = salesYear.Where(f => f.Date.Month == m).Sum(FactTotalFc),
                Appros = ApproFilters.TotalPurchaseFc(approsYear.Where(a => a.Date.Month == m))
            })
            .ToList();

        var totalRevenueYear = salesYear.Sum(FactTotalFc);

        // --- Donut statuts factures du mois ---
        var nbNew = factsMonth.Count(f => f.Status == 0);
        var nbValid = factsMonth.Count(f => f.Status == 1);
        var nbPaid = factsMonth.Count(f => f.Status == 2);
        var nbCancel = factsMonth.Count(f => f.Status == 3);
        var statusData = new List<StatusDataDto>
        {
            new() { Label = "Nouvelles", Count = nbNew },
            new() { Label = "Validees", Count = nbValid },
            new() { Label = "Payees", Count = nbPaid },
            new() { Label = "Annulees", Count = nbCancel },
        };

        // --- Alertes stock (par localisation, depuis T_Stock) ---
        var articles = await ctx.Articles.ToListAsync();
        var lowStockArticles = await ctx.Stocks
            .Include(s => s.Article)
            .Include(s => s.Localisation)
            .Where(s => s.Seuil > 0 && s.Qte <= s.Seuil)
            .OrderBy(s => s.Qte)
            .Take(20)
            .Select(s => new StockAlertDto
            {
                Description = (s.Article != null ? s.Article.Description : s.IdArticle)
                              + " — " + (s.Localisation != null ? s.Localisation.DescriptionLocalisation : "?"),
                Qte = s.Qte,
                Seuil = s.Seuil,
                QteMax = s.QteMax,
                Pct = s.Seuil > 0 ? (double)(s.Qte / s.Seuil) * 100 : 0
            })
            .ToListAsync();

        // --- Top articles vendus ce mois ---
        var factIds = salesMonth.Select(f => f.Id).ToHashSet();
        var detailsMonth = await ctx.FactDetails
            .Where(d => d.IdFact != null && factIds.Contains(d.IdFact.Value))
            .ToListAsync();

        var factTauxById = salesMonth.ToDictionary(
            f => f.Id,
            f => f.Taux > 0 ? f.Taux : latestTaux);

        var articleDict = articles.ToDictionary(a => a.IdArticle, a => a.Description);
        var topArticles = detailsMonth
            .Where(d => d.IdArticle != null && d.IdFact != null)
            .GroupBy(d => d.IdArticle!)
            .Select(g =>
            {
                var montantFc = g.Sum(d =>
                {
                    return (decimal)(d.Qte ?? 0) * (decimal)(d.Pu ?? 0);
                });
                return new
                {
                    IdArticle = g.Key,
                    QteVendue = g.Sum(d => d.Qte ?? 0),
                    Montant = (double)montantFc
                };
            })
            .OrderByDescending(g => g.Montant)
            .Take(8)
            .Select((g, i) => new TopArticleDto
            {
                Rank = i + 1,
                Description = articleDict.GetValueOrDefault(g.IdArticle, g.IdArticle),
                QteVendue = g.QteVendue,
                Montant = g.Montant
            })
            .ToList();

        // --- Dernieres factures ---
        var recentFacts = salesYear
            .OrderByDescending(f => f.DateSys)
            .Take(8)
            .Select(f => new RecentFactDto
            {
                Id = f.Id,
                Client = string.IsNullOrWhiteSpace(f.DescriptionName) ? "\u2014" : f.DescriptionName,
                Date = f.Date,
                Total = FactTotalFc(f),
                StatusLabel = f.Status switch
                {
                    0 => "Nouvelle",
                    1 => "Validee",
                    2 => "Payee",
                    3 => "Annulee",
                    _ => "\u2014"
                }
            })
            .ToList();

        // --- Statistiques rapides ---
        var totalClients = await ctx.Customers.CountAsync();
        var clientsPerm = await ctx.Customers.CountAsync(c => c.IsPermanent == true);
        var totalArticles = articles.Count;
        var totalCommandes = await ctx.Cmds.CountAsync();

        return new DashboardDataDto
        {
            // KPIs
            VentesToday = venteToday,
            VentesTodayFc = venteTodayFc,
            FactsTodayCount = salesToday.Count,
            DisplayTaux = latestTaux,
            TotalApprosMonth = totalAppros,
            ApprosMonthCount = approsMois.Count,
            Benefice = benefice,
            VentesMois = venteMois,
            TrendVente = trendVente,
            TrendVenteUp = trendVenteUp,
            TotalRevenueYear = totalRevenueYear,
            FactsMonthCount = factsMonthCount,
            CmdsEnAttente = cmdsEnAttente,

            // Chart data
            RevenueData = revenueData,
            StatusData = statusData,

            // Lists
            TopArticles = topArticles,
            LowStockArticles = lowStockArticles,
            RecentFacts = recentFacts,

            // Quick stats
            TotalClients = totalClients,
            ClientsPermanents = clientsPerm,
            TotalArticles = totalArticles,
            TotalCommandes = totalCommandes,
        };
    }
}
