using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Common;
using AppPlusPlus.Application.Services.Commandes;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Commandes;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.QueryServices.Commandes;

public class LivraisonQueryService : ILivraisonService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ILivraisonPostingService _posting;

    public LivraisonQueryService(
        IDbContextFactory<AppDbContext> dbFactory,
        ILivraisonPostingService posting)
    {
        _dbFactory = dbFactory;
        _posting = posting;
    }

    public async Task<List<Livraison>> GetLivraisonsByLocalisationsAsync(List<int> localisationIds, string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var livQuery = ctx.Livraisons
            .Include(l => l.Customer)
            .Include(l => l.Commande)
            .Include(l => l.Details)
            .Include(l => l.Fact).ThenInclude(f => f!.Payments)
            .AsQueryable();

        // Filter by user's localisations if not admin (non-empty list = non-admin)
        if (localisationIds.Any())
        {
            livQuery = livQuery.Where(l =>
                (l.UserLogin != null && l.UserLogin.ToLower() == login.ToLower())
                || l.Details.Any(d => d.LocalisationId.HasValue && localisationIds.Contains(d.LocalisationId.Value)));
        }

        return await livQuery
            .OrderByDescending(l => l.Date)
            .ToListAsync();
    }

    public Task DeleteLivraisonAsync(int livraisonId)
        => _posting.DeleteAsync(livraisonId, "system");

    public async Task MarkLivraisonDeliveredAsync(int livraisonId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var liv = await ctx.Livraisons.FindAsync(livraisonId);
        if (liv != null)
        {
            liv.Status = 1;

            if (liv.CommandeId.HasValue)
            {
                var commande = await ctx.Commandes
                    .Include(c => c.Details)
                    .FirstOrDefaultAsync(c => c.CommandeId == liv.CommandeId.Value);

                if (commande != null)
                {
                    var toutLivre = commande.Details.All(d => (d.QteRest ?? 0) <= 0);
                    if (toutLivre && commande.Status < 2)
                        commande.Status = 2;
                    else if (commande.Status < 1)
                        commande.Status = 1;
                }
            }

            await ctx.SaveChangesAsync();
        }
    }

    public async Task<ServiceResult<int>> PayerLivraisonAsync(int livraisonId, string login)
    {
        try
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            // ── Vérification période active ──
            var periode = await ctx.Periodes.FirstOrDefaultAsync(p => p.Activated == true);
            if (periode == null || periode.FromDate == null || periode.ToDate == null)
                return ServiceResult.Fail<int>("Aucune période active n'est définie. Veuillez créer une période avant d'effectuer cette opération.");

            var today = DateTime.Today;
            if (today < periode.FromDate.Value.Date || today > periode.ToDate.Value.Date)
                return ServiceResult.Fail<int>("La date du jour n'est pas comprise dans la période active.");

            // ── Vérification clôture journalière ──
            var todayOnly = DateOnly.FromDateTime(today);
            var userLocIds = await ctx.UserLocalisations
                .Where(ul => ul.UserId == login && ul.LocalisationId.HasValue)
                .Select(ul => ul.LocalisationId!.Value)
                .ToListAsync();
            var hasClosed = await ctx.Versements.AnyAsync(v =>
                v.UserLogin == login
                && v.DateCloture == todayOnly
                && userLocIds.Contains(v.LocalisationId)
                && v.StatutCloture == 0);
            if (hasClosed)
                return ServiceResult.Fail<int>("Clôture en attente d'approbation admin. Aucune transaction n'est possible.");

            var liv = await ctx.Livraisons
                .Include(l => l.Customer)
                .Include(l => l.Details)
                .FirstOrDefaultAsync(l => l.LivraisonId == livraisonId);

            if (liv == null)
                return ServiceResult.Fail<int>("Livraison introuvable.");

            var montant = liv.Details.Sum(d => d.MontantPaye ?? 0);

            var taux = await ctx.TauxChanges.OrderByDescending(t => t.Id).FirstOrDefaultAsync();
            var tauxValue = taux?.TauxValue ?? 1m;

            var customer = liv.ClientId.HasValue
                ? await ctx.Customers.FindAsync(liv.ClientId.Value)
                : null;

            var details = await ctx.LivraisonDetails
                .Where(d => d.LivraisonId == liv.LivraisonId)
                .ToListAsync();

            var articleIds = details.Select(d => d.ArticleId).Where(a => a != null).Distinct().ToList();
            var articleNames = await ctx.Articles
                .Where(a => articleIds.Contains(a.IdArticle))
                .ToDictionaryAsync(a => a.IdArticle, a => a.Description ?? a.IdArticle);

            // ── Create the invoice ──
            var factMoneyId = CurrencyDefaults.MoneyIdCdf;
            var facture = new Fact
            {
                DescriptionName = customer?.CustomerName ?? "Client",
                DescriptionArticle = string.Join(", ", details
                    .Where(d => d.ArticleId != null)
                    .Select(d => articleNames.GetValueOrDefault(d.ArticleId!, d.ArticleId!))),
                Adresse = customer?.Adress,
                Email = customer?.Email,
                Telephone = customer?.Contact,
                Type = 0,
                Date = DateOnly.FromDateTime(DateTime.Today),
                Total = montant,
                TotalApresReduction = montant,
                Reduction = 0,
                TotalReduit = 0,
                Taux = tauxValue,
                Status = 2,
                DateSys = DateTime.Now,
                User = login,
                Cumputer = Environment.MachineName,
                CustomerId = liv.ClientId,
                CommandeId = liv.CommandeId,
                LivraisonId = liv.LivraisonId
            };

            ctx.Facts.Add(facture);
            await ctx.SaveChangesAsync();

            facture.MoneyId = factMoneyId;
            CurrencyFormat.ApplySnapshot(facture, CurrencyFormat.FromFactStoredTotal(montant, factMoneyId, tauxValue));
            ctx.Entry(facture).Property(f => f.Montant).IsModified = true;
            ctx.Entry(facture).Property(f => f.MontantApresConversion).IsModified = true;

            // ── Create invoice detail lines ──
            foreach (var d in details)
            {
                var pu = (d.Qte.HasValue && d.Qte > 0) ? (d.MontantPaye ?? 0) / d.Qte.Value : 0;
                var detail = new FactDetail
                {
                    IdFact = facture.Id,
                    IdArticle = d.ArticleId,
                    Qte = (double?)(d.Qte ?? 0),
                    Pu = (double?)pu,
                    Status = 1,
                    Localisationid = d.LocalisationId
                };
                var amounts = CurrencyFormat.ComputeMonetarySnapshot(
                    pu, (decimal)(d.Qte ?? 0), factMoneyId, tauxValue, puStoredInCdf: true);
                CurrencyFormat.ApplySnapshot(detail, amounts);
                ctx.FactDetails.Add(detail);
            }

            // ── Create the payment ──
            var paySnap = CurrencyFormat.FromFactStoredTotal(montant, factMoneyId, tauxValue);
            var payment = new Payment
            {
                IdFact = facture.Id,
                Date = DateOnly.FromDateTime(DateTime.Today),
                Note = $"Paiement livraison N\u00b0 {liv.LivraisonId}",
                DateSys = DateTime.Now,
                User = login
            };
            CurrencyFormat.ApplySnapshot(payment, paySnap);
            ctx.Payments.Add(payment);

            // ── Update livraison status ──
            var livEntity = await ctx.Livraisons.FindAsync(liv.LivraisonId);
            if (livEntity != null) livEntity.Status = 2;

            // ── Update commande if linked ──
            if (liv.CommandeId.HasValue)
            {
                var commande = await ctx.Commandes
                    .Include(c => c.Details)
                    .FirstOrDefaultAsync(c => c.CommandeId == liv.CommandeId.Value);

                if (commande != null)
                {
                    commande.MontantPaye += montant;
                    commande.MontantRest = commande.MontantTotal - commande.MontantPaye;
                    if (commande.MontantRest < 0) commande.MontantRest = 0;

                    var allLivraisonsPaid = await ctx.Livraisons
                        .Where(l => l.CommandeId == commande.CommandeId)
                        .AllAsync(l => l.Status == 2);
                    var toutLivre = commande.Details.All(d => (d.QteRest ?? 0) <= 0);

                    if (allLivraisonsPaid && toutLivre)
                        commande.Status = 3;
                }
            }

            await ctx.SaveChangesAsync();

            return ServiceResult.Ok(facture.Id, $"Facture F-{facture.Id} cr\u00e9\u00e9e - Livraison N\u00b0 {liv.LivraisonId} pay\u00e9e");
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return ServiceResult.Fail<int>(msg);
        }
    }
}
