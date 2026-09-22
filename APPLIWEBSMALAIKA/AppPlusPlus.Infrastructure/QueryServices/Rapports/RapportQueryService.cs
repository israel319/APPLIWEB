using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.DTOs.Rapports;
using AppPlusPlus.Application.Services.Rapports;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.QueryServices.Rapports;

public class RapportQueryService : IRapportQueryService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public RapportQueryService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<decimal> GetLatestTauxAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await ctx.TauxChanges
            .OrderByDescending(t => t.Id)
            .Select(t => t.TauxValue)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BilanReportResult> GetBilanAsync(
        DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var articles = await ctx.Articles.ToDictionaryAsync(
            a => a.IdArticle, a => a.Description ?? a.IdArticle, cancellationToken);

        var facts = await ctx.Facts
            .Where(f => f.Date >= from && f.Date <= to && f.Status != 3 && f.Status != 0)
            .Include(f => f.Details)
            .OrderByDescending(f => f.Date)
            .ToListAsync(cancellationToken);

        var latestTaux = await GetLatestTauxInternalAsync(ctx, cancellationToken);

        decimal FactUsd(Fact f) =>
            CurrencyFormat.CdfStoredToUsd(f.TotalApresReduction ?? f.Total ?? 0, f.Taux > 0 ? f.Taux : latestTaux);

        var venteRows = facts.Select(f =>
        {
            var artNames = f.Details.Any()
                ? string.Join(", ", f.Details
                    .Select(d => articles.GetValueOrDefault(d.IdArticle ?? "", d.IdArticle ?? ""))
                    .Where(n => !string.IsNullOrEmpty(n)).Distinct())
                : f.DescriptionArticle;
            return new BilanRowDto
            {
                Date = f.Date,
                Type = "Vente",
                Reference = $"F-{f.Id:D6}",
                Description = f.DescriptionName + (string.IsNullOrEmpty(artNames) ? "" : $" — {artNames}"),
                Qte = (decimal)f.Details.Sum(d => d.Qte ?? 0),
                Montant = FactUsd(f),
                Utilisateur = f.User ?? ""
            };
        });

        var appros = await ctx.Appros
            .Where(a => a.Date >= from && a.Date <= to)
            .OrderByDescending(a => a.Date)
            .ToListAsync(cancellationToken);

        var vraisAppros = ApproFilters.CountedPurchases(appros).ToList();
        var transferts = appros.Where(ApproFilters.IsInternalTransfer).ToList();

        var approRows = vraisAppros.Select(a => new BilanRowDto
        {
            Date = a.Date,
            Type = "Appro",
            Reference = $"A-{a.Id:D6}",
            Description = articles.GetValueOrDefault(a.IdArticle, a.IdArticle),
            Qte = a.Qte,
            Montant = (a.PA ?? 0) * a.Qte,
            Utilisateur = a.User ?? ""
        });

        var transfertRows = transferts.Select(a => new BilanRowDto
        {
            Date = a.Date,
            Type = "Transfert",
            Reference = $"T-{a.Id:D6}",
            Description = articles.GetValueOrDefault(a.IdArticle, a.IdArticle) + $" ({a.Commentaire})",
            Qte = a.Qte,
            Montant = 0,
            Utilisateur = a.User ?? ""
        });

        var rows = venteRows.Concat(approRows).Concat(transfertRows)
            .OrderByDescending(r => r.Date)
            .Select((r, i) => { r.RowId = i; return r; })
            .ToList();

        return new BilanReportResult
        {
            Rows = rows,
            NbFactures = facts.Count,
            TotalVentes = facts.Sum(FactUsd),
            TotalAppros = ApproFilters.TotalPurchaseFc(vraisAppros),
            Benefice = ApproFilters.TotalBenefitFc(vraisAppros),
            LatestTaux = latestTaux
        };
    }

    public async Task<IReadOnlyList<HistoriqueReportRowDto>> GetHistoriqueAsync(
        DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var articles = await ctx.Articles.ToDictionaryAsync(
            a => a.IdArticle, a => a.Description ?? a.IdArticle, cancellationToken);

        var facts = await ctx.Facts
            .Where(f => f.Date >= from && f.Date <= to && f.Status != 0)
            .OrderByDescending(f => f.DateSys)
            .ToListAsync(cancellationToken);

        var venteRows = facts.Select(f =>
        {
            var (lbl, _) = f.Status switch
            {
                0 => ("Brouillon", 0),
                1 => ("Validée", 1),
                2 => ("Payée", 2),
                3 => ("Annulée", 3),
                _ => ("—", -1)
            };
            return new HistoriqueReportRowDto
            {
                DateOp = f.DateSys,
                Date = f.Date,
                Type = "Vente",
                Reference = $"F-{f.Id:D6}",
                Description = f.DescriptionName + (string.IsNullOrEmpty(f.DescriptionArticle) ? "" : $" — {f.DescriptionArticle}"),
                Montant = f.TotalApresReduction ?? f.Total ?? 0,
                StatusCode = f.Status,
                StatusLabel = lbl,
                Utilisateur = f.User ?? "",
                FactId = f.Id
            };
        });

        var appros = await ctx.Appros
            .Where(a => a.Date >= from && a.Date <= to)
            .OrderByDescending(a => a.DateSys)
            .ToListAsync(cancellationToken);

        var approRows = ApproFilters.CountedPurchases(appros).Select(a => new HistoriqueReportRowDto
        {
            DateOp = a.DateSys,
            Date = a.Date,
            Type = "Appro",
            Reference = $"A-{a.Id:D6}",
            Description = articles.GetValueOrDefault(a.IdArticle, a.IdArticle),
            Montant = ApproFilters.PurchaseAmountFc(a),
            StatusCode = 1,
            StatusLabel = "Effectué",
            Utilisateur = a.User ?? "",
            FactId = null
        });

        var approCancelledRows = appros.Where(ApproFilters.IsCancelled).Select(a => new HistoriqueReportRowDto
        {
            DateOp = a.DateSys,
            Date = a.Date,
            Type = "Appro",
            Reference = $"A-{a.Id:D6}",
            Description = articles.GetValueOrDefault(a.IdArticle, a.IdArticle) + " (annulé)",
            Montant = ApproFilters.PurchaseAmountFc(a),
            StatusCode = 3,
            StatusLabel = "Annulé",
            Utilisateur = a.User ?? "",
            FactId = null
        });

        return venteRows.Concat(approRows).Concat(approCancelledRows)
            .OrderByDescending(r => r.DateOp)
            .Select((r, i) => { r.RowId = i; return r; })
            .ToList();
    }

    public async Task<IReadOnlyList<VenteLigneReportDto>> GetVentesLignesAsync(
        DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await ctx.FactDetails
            .Include(d => d.Fact)
            .Include(d => d.Article)
            .Where(d => d.Fact != null && d.Fact.Date >= from && d.Fact.Date <= to && d.Fact.Status != 0)
            .OrderBy(d => d.Fact!.Date)
            .ThenBy(d => d.Fact!.Id)
            .Select(d => new VenteLigneReportDto(
                d.Fact!.Id,
                d.Fact.DescriptionName ?? "",
                d.Article != null ? d.Article.Description : (d.IdArticle ?? ""),
                d.Qte ?? 0,
                d.Montant,
                d.MoneyId,
                d.Taux,
                d.MontantApresConversion,
                d.Pu ?? 0,
                (d.Qte ?? 0) * (d.Pu ?? 0),
                d.Fact.Montant,
                d.Fact.MoneyId,
                d.Fact.MontantApresConversion,
                d.Fact.TotalApresReduction ?? d.Fact.Total ?? 0,
                d.Fact.Reduction ?? 0,
                d.Fact.Taux,
                d.Fact.Date,
                d.Fact.User ?? ""))
            .ToListAsync(cancellationToken);
    }

    public async Task<FacturesMoisReportResult> GetFacturesMoisAsync(
        int year, int month, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        var latestTaux = await GetLatestTauxInternalAsync(ctx, cancellationToken);

        var facts = await ctx.Facts
            .Where(f => f.Date >= from && f.Date <= to && f.Status != 0)
            .OrderByDescending(f => f.Date)
            .ToListAsync(cancellationToken);

        decimal FactUsd(Fact f) =>
            CurrencyFormat.CdfStoredToUsd(f.Total ?? 0, f.Taux > 0 ? f.Taux : latestTaux);
        decimal RedUsd(Fact f) =>
            CurrencyFormat.CdfStoredToUsd(f.Reduction ?? 0, f.Taux > 0 ? f.Taux : latestTaux);
        decimal NetUsd(Fact f) =>
            CurrencyFormat.CdfStoredToUsd(f.TotalApresReduction ?? 0, f.Taux > 0 ? f.Taux : latestTaux);

        var rows = facts.Select(f => new FactMoisRowDto
        {
            Id = f.Id,
            DescriptionName = f.DescriptionName,
            DescriptionArticle = f.DescriptionArticle,
            Total = f.Total,
            Reduction = f.Reduction,
            TotalApresReduction = f.TotalApresReduction,
            Date = f.Date,
            Status = f.Status,
            User = f.User,
            Taux = f.Taux
        }).ToList();

        return new FacturesMoisReportResult
        {
            Factures = rows,
            TotalFactures = facts.Count,
            TotalMontantUsd = facts.Sum(FactUsd),
            TotalReductionUsd = facts.Sum(RedUsd),
            TotalNetUsd = facts.Sum(NetUsd),
            LatestTaux = latestTaux
        };
    }

    public async Task<IReadOnlyList<StockReportRowDto>> GetStockAsync(
        IReadOnlyList<int> localisationIds, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var query = ctx.Stocks
            .Include(s => s.Article)
            .Include(s => s.Localisation)
            .AsQueryable();

        if (localisationIds.Count > 0)
            query = query.Where(s => localisationIds.Contains(s.IdLocalisation));

        return await query
            .OrderBy(s => s.Article!.Description)
            .ThenBy(s => s.Localisation!.DescriptionLocalisation)
            .Select(s => new StockReportRowDto(
                s.IdArticle,
                s.Article != null ? s.Article.Description : s.IdArticle,
                s.Article != null ? s.Article.Price : 0,
                s.Qte,
                s.Seuil,
                s.QteMax,
                (double)s.Qte * (s.Article != null ? s.Article.Price : 0),
                s.Localisation != null ? s.Localisation.DescriptionLocalisation ?? "?" : "?"))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CommandeReportRowDto>> GetCommandesFournisseurAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var query = ctx.Cmds.Include(c => c.Supplier).Include(c => c.Details).AsQueryable();
        if (from.HasValue) query = query.Where(c => c.DateCommande >= from.Value);
        if (to.HasValue) query = query.Where(c => c.DateCommande <= to.Value);

        var cmds = await query.OrderByDescending(c => c.DateCommande).ToListAsync(cancellationToken);

        var articleIds = cmds.SelectMany(c => c.Details)
            .Select(d => d.IdArticle)
            .Where(id => id != null)
            .Distinct()
            .ToList();

        var articleNames = articleIds.Count > 0
            ? await ctx.Articles
                .Where(a => articleIds.Contains(a.IdArticle))
                .ToDictionaryAsync(a => a.IdArticle, a => a.Description ?? a.IdArticle, cancellationToken)
            : new Dictionary<string, string>();

        return cmds.Select(c =>
        {
            var arts = c.Details.Select(d => new CommandeArticleReportDto
            {
                Name = articleNames.GetValueOrDefault(d.IdArticle ?? "", d.IdArticle ?? "?") ?? d.IdArticle ?? "?",
                QteOrder = (decimal)(d.QteOrder ?? 0),
                QteReceived = (decimal)(d.QteReceive ?? 0)
            }).ToList();

            return new CommandeReportRowDto
            {
                Id = c.Id,
                Fournisseur = c.Supplier?.SupplierName ?? "—",
                DateCommande = c.DateCommande,
                Status = c.Status ?? 0,
                User = c.User ?? "—",
                NbArticles = arts.Count,
                TotalOrder = arts.Sum(a => a.QteOrder),
                TotalReceived = arts.Sum(a => a.QteReceived),
                Articles = arts
            };
        }).ToList();
    }

    public async Task<ApproMargesReportResult> GetApproMargesAsync(
        DateOnly? from, DateOnly? to, IReadOnlyList<int> localisationIds,
        CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var query = ctx.Appros.AsQueryable();
        if (from.HasValue) query = query.Where(a => a.Date >= from.Value);
        if (to.HasValue) query = query.Where(a => a.Date <= to.Value);

        var appros = ApproFilters.CountedPurchases(await query.OrderByDescending(a => a.Date).ToListAsync(cancellationToken)).ToList();
        var articleIds = appros.Select(a => a.IdArticle).Distinct().ToList();

        var articles = articleIds.Count > 0
            ? await ctx.Articles.Where(a => articleIds.Contains(a.IdArticle)).ToListAsync(cancellationToken)
            : new List<Domain.Entities.Catalogue.Article>();

        var artDict = articles.ToDictionary(a => a.IdArticle, a => a);
        var latestTaux = await GetLatestTauxInternalAsync(ctx, cancellationToken);

        var stockQuery = ctx.Stocks.AsQueryable();
        if (articleIds.Count > 0)
            stockQuery = stockQuery.Where(s => articleIds.Contains(s.IdArticle));
        if (localisationIds.Count > 0)
            stockQuery = stockQuery.Where(s => localisationIds.Contains(s.IdLocalisation));

        var stocks = await stockQuery.ToListAsync(cancellationToken);

        var rows = appros.Select(a =>
        {
            artDict.TryGetValue(a.IdArticle, out var art);
            var isUsd = art == null || !CurrencyDefaults.IsCdfMoneyId(art.IdMonais);
            var taux = a.Taux ?? latestTaux;
            if (taux <= 0) taux = latestTaux;
            var pa = a.PA ?? 0;
            var depense = pa * a.Qte;
            var stockActuel = a.LocalisationId.HasValue
                ? stocks.Where(s => s.IdArticle == a.IdArticle && s.IdLocalisation == a.LocalisationId.Value).Sum(s => s.Qte)
                : stocks.Where(s => s.IdArticle == a.IdArticle).Sum(s => s.Qte);

            return new ApproMargeReportRowDto
            {
                Id = a.Id,
                ArticleName = art?.Description ?? a.IdArticle,
                Qte = a.Qte,
                PA = pa,
                PV = a.PV ?? 0,
                Depense = depense,
                StockActuel = stockActuel,
                Ben = a.Ben ?? 0,
                BenTotal = a.BenTotal ?? 0,
                IsUsd = isUsd,
                Taux = taux,
                Date = a.Date,
                User = a.User ?? ""
            };
        }).ToList();

        return new ApproMargesReportResult
        {
            Rows = rows,
            LatestTaux = latestTaux,
            TotalBeneficeUsd = rows.Sum(b => b.BenTotal_Usd)
        };
    }

    public async Task<IReadOnlyList<MouvementReportRowDto>> GetMouvementsAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var articles = await ctx.Articles.ToDictionaryAsync(
            a => a.IdArticle, a => a.Description ?? a.IdArticle, cancellationToken);

        var approQuery = ctx.Appros.AsQueryable();
        if (from.HasValue) approQuery = approQuery.Where(a => a.Date >= from.Value);
        if (to.HasValue) approQuery = approQuery.Where(a => a.Date <= to.Value);

        var appros = ApproFilters.CountedPurchases(await approQuery.ToListAsync(cancellationToken)).ToList();

        var entrees = appros.Select(a => new MouvementReportRowDto
        {
            IdArticle = a.IdArticle,
            Description = articles.GetValueOrDefault(a.IdArticle, a.IdArticle),
            Qte = a.Qte,
            Pu = a.PA ?? 0,
            Date = a.Date,
            Mouvement = "Entrée",
            Utilisateur = a.User ?? ""
        });

        var fdQuery = ctx.FactDetails
            .Include(d => d.Fact)
            .Where(d => d.Fact != null && d.Fact.Status != 0);

        if (from.HasValue) fdQuery = fdQuery.Where(d => d.Fact!.Date >= from.Value);
        if (to.HasValue) fdQuery = fdQuery.Where(d => d.Fact!.Date <= to.Value);

        var fds = await fdQuery.ToListAsync(cancellationToken);

        var sorties = fds.Select(d => new MouvementReportRowDto
        {
            IdArticle = d.IdArticle ?? "",
            Description = articles.GetValueOrDefault(d.IdArticle ?? "", d.IdArticle ?? ""),
            Qte = (decimal)(d.Qte ?? 0),
            Pu = (decimal)(d.Pu ?? 0),
            Date = d.Fact?.Date ?? DateOnly.MinValue,
            Mouvement = "Sortie",
            Utilisateur = d.Fact?.User ?? ""
        });

        return entrees.Concat(sorties)
            .OrderByDescending(m => m.Date)
            .Select((m, i) => { m.RowId = i; return m; })
            .ToList();
    }

    static async Task<decimal> GetLatestTauxInternalAsync(AppDbContext ctx, CancellationToken cancellationToken) =>
        await ctx.TauxChanges
            .OrderByDescending(t => t.Id)
            .Select(t => t.TauxValue)
            .FirstOrDefaultAsync(cancellationToken);
}
