using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.DTOs.Approvisionnement;
using AppPlusPlus.Application.Services.Approvisionnement;
using AppPlusPlus.Domain.Approvisionnement;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Approvisionnement;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.QueryServices;

public class ApproQueryService : IApproQueryService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ApproQueryService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<ApproListItemDto>> GetListAsync(
        IReadOnlyList<int> localisationIds,
        CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var approsQuery = ctx.Appros
            .Include(a => a.Details)
            .AsNoTracking()
            .AsQueryable();

        if (localisationIds.Count > 0)
        {
            approsQuery = approsQuery.Where(a =>
                (a.LocalisationId.HasValue && localisationIds.Contains(a.LocalisationId.Value)) ||
                a.Details.Any(d => localisationIds.Contains(d.IdLocalisation)));
        }

        var appros = await approsQuery.OrderByDescending(a => a.Date).ToListAsync(cancellationToken);

        var articleIds = appros
            .SelectMany(a => a.Details.Any()
                ? a.Details.Select(d => d.IdArticle)
                : new[] { a.IdArticle })
            .Distinct()
            .ToList();

        var artDict = await ctx.Articles.AsNoTracking()
            .Where(a => articleIds.Contains(a.IdArticle))
            .ToDictionaryAsync(a => a.IdArticle, a => a, cancellationToken);

        var latestTaux = await ctx.TauxChanges.AsNoTracking()
            .OrderByDescending(t => t.Id)
            .Select(t => t.TauxValue)
            .FirstOrDefaultAsync(cancellationToken);

        var locDict = await ctx.Localisations.AsNoTracking()
            .ToDictionaryAsync(
                l => l.IdLocalisation,
                l => string.IsNullOrWhiteSpace(l.DescriptionLocalisation)
                    ? $"#{l.IdLocalisation}"
                    : l.DescriptionLocalisation,
                cancellationToken);

        return appros.Select(a => MapToDto(a, artDict, locDict, latestTaux)).ToList();
    }

    public async Task<ApproDetailDto?> GetDetailAsync(int approId, CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var appro = await ctx.Appros
            .Include(a => a.Details)
            .Include(a => a.Supplier)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == approId, cancellationToken);

        if (appro is null) return null;

        var articleIds = appro.Details.Any()
            ? appro.Details.Select(d => d.IdArticle).Distinct().ToList()
            : new List<string> { appro.IdArticle };

        var artDict = await ctx.Articles.AsNoTracking()
            .Where(a => articleIds.Contains(a.IdArticle))
            .ToDictionaryAsync(a => a.IdArticle, a => a.Description ?? a.IdArticle, cancellationToken);

        var locDict = await ctx.Localisations.AsNoTracking()
            .ToDictionaryAsync(
                l => l.IdLocalisation,
                l => string.IsNullOrWhiteSpace(l.DescriptionLocalisation)
                    ? $"#{l.IdLocalisation}"
                    : l.DescriptionLocalisation!,
                cancellationToken);

        var latestTaux = await ctx.TauxChanges.AsNoTracking()
            .OrderByDescending(t => t.Id)
            .Select(t => t.TauxValue)
            .FirstOrDefaultAsync(cancellationToken);

        var taux = MonetaryStandard.ResolveTaux(appro.Taux ?? latestTaux);
        var details = appro.Details?.ToList() ?? new List<ApproDetail>();
        var metrics = ApproDomainRules.Aggregate(appro, details);

        var locId = appro.LocalisationId ?? details.FirstOrDefault()?.IdLocalisation;
        var locLabel = locId.HasValue && locDict.TryGetValue(locId.Value, out var ln) ? ln : "";

        var lines = details.Count > 0
            ? details.Select(d =>
            {
                var pa = d.PA ?? 0;
                var pv = d.PV ?? 0;
                var achat = pa * d.Qte;
                var vente = pv * d.Qte;
                return new ApproDetailLineDto
                {
                    Id = d.Id,
                    IdArticle = d.IdArticle,
                    ArticleName = artDict.GetValueOrDefault(d.IdArticle, d.IdArticle),
                    LocalisationLabel = locDict.GetValueOrDefault(d.IdLocalisation, "—"),
                    Qte = d.Qte,
                    PA = d.PA,
                    PV = d.PV,
                    MontantAchatFc = achat,
                    MontantVenteFc = vente,
                    BenLineFc = d.BenTotal ?? (pv - pa) * d.Qte,
                    DateExpiration = d.DateExpiration
                };
            }).OrderBy(l => l.ArticleName).ToList()
            : new List<ApproDetailLineDto>
            {
                new()
                {
                    IdArticle = appro.IdArticle,
                    ArticleName = artDict.GetValueOrDefault(appro.IdArticle, appro.IdArticle),
                    LocalisationLabel = locLabel,
                    Qte = appro.Qte,
                    PA = appro.PA,
                    PV = appro.PV,
                    MontantAchatFc = (appro.PA ?? 0) * appro.Qte,
                    MontantVenteFc = (appro.PV ?? 0) * appro.Qte,
                    BenLineFc = appro.BenTotal ?? ((appro.PV ?? 0) - (appro.PA ?? 0)) * appro.Qte
                }
            };

        return new ApproDetailDto
        {
            Id = appro.Id,
            Date = appro.Date,
            DateSys = appro.DateSys,
            Reference = appro.Reference,
            Commentaire = appro.Commentaire,
            Taux = taux,
            User = appro.User,
            LocalisationLabel = locLabel,
            SupplierName = appro.Supplier?.SupplierName,
            IsFromCommand = (appro.IdCmd.HasValue && appro.IdCmd.Value > 0) || appro.IdCmdDetail > 0,
            CommandId = appro.IdCmd,
            IsCancelled = ApproFilters.IsCancelled(appro),
            IsTransfer = ApproFilters.IsInternalTransfer(appro),
            LineCount = metrics.LineCount,
            TotalQte = metrics.Qte,
            TotalAchatFc = lines.Sum(l => l.MontantAchatFc),
            TotalVenteFc = lines.Sum(l => l.MontantVenteFc),
            BenTotalFc = lines.Sum(l => l.BenLineFc),
            Lines = lines
        };
    }

    static ApproListItemDto MapToDto(
        Appro a,
        IReadOnlyDictionary<string, Article> artDict,
        IReadOnlyDictionary<int, string> locDict,
        decimal latestTaux)
    {
        var details = a.Details?.ToList() ?? new List<ApproDetail>();
        var metrics = ApproDomainRules.Aggregate(a, details);

        string articleName;
        if (details.Count > 0)
        {
            var primaryId = details[0].IdArticle;
            artDict.TryGetValue(primaryId, out var primaryArt);
            articleName = ApproDomainRules.FormatArticleLabel(
                primaryId, primaryArt?.Description, metrics.LineCount);
        }
        else
        {
            artDict.TryGetValue(a.IdArticle, out var art);
            articleName = art?.Description ?? a.IdArticle;
        }

        var locId = a.LocalisationId ?? details.FirstOrDefault()?.IdLocalisation;
        var locLabel = locId.HasValue && locDict.TryGetValue(locId.Value, out var locName) ? locName : "";

        var totalAchat = details.Count > 0
            ? details.Sum(d => (d.PA ?? 0) * d.Qte)
            : (a.PA ?? 0) * a.Qte;
        var benTotal = details.Count > 0
            ? details.Sum(d => d.BenTotal ?? ((d.PV ?? 0) - (d.PA ?? 0)) * d.Qte)
            : a.BenTotal ?? metrics.Ben;

        return new ApproListItemDto
        {
            Id = a.Id,
            ArticleName = articleName,
            Qte = metrics.Qte,
            PA = metrics.PA,
            PV = metrics.PV,
            Ben = metrics.Ben,
            TotalAchatFc = totalAchat,
            BenTotalFc = benTotal,
            Commentaire = a.Commentaire,
            IsUsd = false,
            Taux = MonetaryStandard.ResolveTaux(a.Taux ?? latestTaux),
            Date = a.Date,
            User = a.User,
            Reference = a.Reference,
            LocalisationLabel = locLabel,
            IsFromCommand = (a.IdCmd.HasValue && a.IdCmd.Value > 0) || a.IdCmdDetail > 0,
            CommandId = a.IdCmd,
            IsCancelled = ApproFilters.IsCancelled(a),
            IsTransfer = ApproFilters.IsInternalTransfer(a),
            StatusId = a.StatusId,
            LineCount = metrics.LineCount
        };
    }
}
