using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Common;
using AppPlusPlus.Application.DTOs.Vente;
using AppPlusPlus.Application.Services.Vente;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Infrastructure.Persistence;
using AppPlusPlus.Infrastructure.Services;

namespace AppPlusPlus.Infrastructure.QueryServices.Vente;

public class FacturationQueryService : IFacturationService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly StockMovementService _movements;

    public FacturationQueryService(
        IDbContextFactory<AppDbContext> dbFactory,
        StockMovementService movements)
    {
        _dbFactory = dbFactory;
        _movements = movements;
    }

    public async Task<List<FactRowDto>> GetFactureRowsAsync(List<int> localisationIds, string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);

        var articleNames = await ctx.Articles
            .ToDictionaryAsync(a => a.IdArticle, a => a.Description ?? a.IdArticle);

        var factsQuery = ctx.Facts
            .Include(f => f.Details)
            .Where(f => f.Date >= yesterday && f.Date <= today)
            .AsQueryable();

        List<Domain.Entities.Finance.Versement> versements = new();
        if (localisationIds.Any())
        {
            versements = await ctx.Versements
                .Where(v => localisationIds.Contains(v.LocalisationId))
                .Where(v => v.DateCloture >= yesterday && v.DateCloture <= today)
                .ToListAsync();

            factsQuery = factsQuery.Where(f =>
                f.User.ToLower() == login.ToLower()
                || f.Details.Any(d => d.Localisationid.HasValue && localisationIds.Contains(d.Localisationid.Value)));
        }

        var facts = await factsQuery
            .OrderByDescending(f => f.Date)
            .ThenByDescending(f => f.Id)
            .ToListAsync();

        var factIds = facts.Select(f => f.Id).ToList();
        var paymentsByFact = factIds.Count == 0
            ? new Dictionary<int, decimal>()
            : await ctx.Payments
                .Where(p => factIds.Contains(p.IdFact))
                .GroupBy(p => p.IdFact)
                .Select(g => new { IdFact = g.Key, Total = g.Sum(x => x.Montant) })
                .ToDictionaryAsync(x => x.IdFact, x => x.Total);

        return facts.Select(f =>
        {
            var articlesStr = f.Details.Any()
                ? string.Join(", ", f.Details
                    .Where(d => d.IdArticle != null)
                    .Select(d => articleNames.GetValueOrDefault(d.IdArticle!, d.IdArticle!)))
                : f.DescriptionArticle ?? "";

            var total = (double)(f.TotalApresReduction ?? f.Total ?? 0);
            paymentsByFact.TryGetValue(f.Id, out var payeDec);
            ResolvePaymentAmounts(f, payeDec, out var totalPaye, out var reste);

            var factLocs = f.Details
                .Where(d => d.Localisationid.HasValue)
                .Select(d => d.Localisationid!.Value);
            var (estCloturee, apresCloture, clotureEnAttente) = ClotureSegmentHelper.ResolveForFact(
                f.Date, f.DateSys, factLocs, localisationIds, versements);

            return new FactRowDto
            {
                Id = f.Id,
                Client = f.DescriptionName,
                Articles = articlesStr,
                TotalQte = f.Details.Sum(d => d.Qte ?? 0),
                Total = total,
                Paye = (double)totalPaye,
                Solde = (double)reste,
                Date = f.Date,
                Status = f.Status,
                Login = f.User,
                Taux = f.Taux > 0 ? f.Taux : 0,
                EstCloturee = estCloturee,
                ApresCloture = apresCloture,
                ClotureEnAttente = clotureEnAttente,
            };
        }).ToList();
    }

    public async Task<ServiceResult> DeleteFactureAsync(int factId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        var periode = await ctx.Periodes.FirstOrDefaultAsync(p => p.Activated == true);
        if (periode == null || periode.FromDate == null || periode.ToDate == null)
            return ServiceResult.Fail("Aucune période active n'est définie. Veuillez créer une période avant d'effectuer cette opération.");

        var today = DateTime.Today;
        if (today < periode.FromDate.Value.Date || today > periode.ToDate.Value.Date)
            return ServiceResult.Fail("La date du jour n'est pas comprise dans la période active.");

        var fact = await ctx.Facts.Include(f => f.Details).FirstOrDefaultAsync(f => f.Id == factId);
        if (fact == null)
            return ServiceResult.Ok("Facture supprimée avec succès.");

        if (fact.Status >= 1)
        {
            await _movements.TryReverseDocumentAsync(
                ctx,
                TypesDocument.FACTURE,
                factId,
                fact.User ?? "system",
                $"Suppression facture N°{factId}",
                DateTime.Now);
        }

        var payments = await ctx.Payments.Where(p => p.IdFact == factId).ToListAsync();
        ctx.Payments.RemoveRange(payments);
        ctx.FactDetails.RemoveRange(fact.Details);
        ctx.Facts.Remove(fact);

        await ctx.SaveChangesAsync();
        await tx.CommitAsync();

        return ServiceResult.Ok("Facture supprimée avec succès.");
    }

    public async Task<List<FactureViewDto>> GetPaiementsAsync(List<int> localisationIds, string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);

        var facturesQuery = ctx.Facts
            .Include(f => f.Customer)
            .Include(f => f.Money)
            .Include(f => f.Payments)
            .Include(f => f.Details)
            .Where(f => f.Status >= 1 && f.Status <= 2)
            .Where(f => f.Date >= yesterday && f.Date <= today)
            .AsQueryable();

        // Filter by user's localisations if not admin
        if (localisationIds.Any())
        {
            facturesQuery = facturesQuery.Where(f =>
                f.User.ToLower() == login.ToLower()
                || f.Details.Any(d => d.Localisationid.HasValue && localisationIds.Contains(d.Localisationid.Value)));
        }

        var factures = await facturesQuery
            .OrderByDescending(f => f.DateSys)
            .ToListAsync();

        return factures.Select(f =>
        {
            var payeFromDb = f.Payments?.Sum(p => p.Montant) ?? 0;
            ResolvePaymentAmounts(f, payeFromDb, out var totalPaye, out var reste);
            return new FactureViewDto
            {
                Facture = f,
                ClientName = f.Customer?.CustomerName ?? f.DescriptionName,
                TotalPaye = totalPaye,
                Reste = reste,
                IsDirectSale = !f.CommandeId.HasValue,
                Payments = f.Payments?.OrderByDescending(p => p.Date).ToList() ?? new()
            };
        }).ToList();
    }

    /// <summary>
    /// Ventes directes (POS) : considérées payées à la validation même sans ligne T_Payments (données legacy).
    /// Commandes : montant réel des paiements enregistrés.
    /// </summary>
    private static void ResolvePaymentAmounts(Fact f, decimal paymentsSum, out decimal totalPaye, out decimal reste)
    {
        var totalFacture = f.TotalApresReduction ?? f.Total ?? 0;
        totalPaye = paymentsSum;

        if (!f.CommandeId.HasValue && f.Status is 1 or 2 && totalPaye < totalFacture)
            totalPaye = totalFacture;

        reste = Math.Max(0, totalFacture - totalPaye);
    }

    public async Task<Fact?> GetFactureWithDetailsAsync(int factId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Facts
            .Include(f => f.Money)
            .Include(f => f.Details).ThenInclude(d => d.Article)
            .FirstOrDefaultAsync(f => f.Id == factId);
    }

    public async Task<OperatorDailySalesDto> GetOperatorDailySalesAsync(string login, DateOnly date)
    {
        var byDate = await GetOperatorDailySalesByDatesAsync(login, date);
        return byDate.GetValueOrDefault(date) ?? new OperatorDailySalesDto();
    }

    public async Task<OperatorSalesSummaryDto> GetOperatorSalesSummaryAsync(string login)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);
        var byDate = await GetOperatorDailySalesByDatesAsync(login, yesterday, today);

        return new OperatorSalesSummaryDto
        {
            Yesterday = byDate.GetValueOrDefault(yesterday) ?? new OperatorDailySalesDto(),
            Today = byDate.GetValueOrDefault(today) ?? new OperatorDailySalesDto()
        };
    }

    private async Task<Dictionary<DateOnly, OperatorDailySalesDto>> GetOperatorDailySalesByDatesAsync(
        string login, params DateOnly[] dates)
    {
        var result = dates.Distinct().ToDictionary(d => d, _ => new OperatorDailySalesDto());
        if (string.IsNullOrWhiteSpace(login) || dates.Length == 0)
            return result;

        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var loginLower = login.ToLower();
        var dateSet = dates.ToHashSet();

        var payments = await ctx.Payments
            .Where(p => dateSet.Contains(p.Date) && p.User != null && p.User.ToLower() == loginLower)
            .Select(p => new { p.Montant, p.IdFact, p.Date })
            .ToListAsync();

        if (payments.Count == 0)
            return result;

        var factIds = payments.Select(p => p.IdFact).Distinct().ToList();
        var facts = await ctx.Facts
            .Include(f => f.Money)
            .Where(f => factIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id);

        foreach (var p in payments)
        {
            if (!facts.TryGetValue(p.IdFact, out var fact))
                continue;

            var usd = PaymentToUsd(p.Montant, fact);
            var dto = result[p.Date];
            dto.CollectedUsd += usd;
            dto.PaymentCount++;
        }

        return result;
    }

    private static decimal PaymentToUsd(decimal amount, Fact fact)
    {
        var isUsd = CurrencyFormat.IsUsd(fact.Money?.DescriptionMonais);
        var taux = fact.Taux > 0 ? fact.Taux : 2800m;
        return isUsd ? amount : CurrencyFormat.CdfStoredToUsd(amount, taux);
    }
}
