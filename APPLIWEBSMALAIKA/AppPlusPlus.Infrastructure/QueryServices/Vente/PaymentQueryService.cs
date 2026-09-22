using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Common;
using AppPlusPlus.Application.Services.Vente;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.QueryServices.Vente;

public class PaymentQueryService : IPaymentService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public PaymentQueryService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Payment>> GetPaymentsByFactureAsync(int factId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        return await ctx.Payments.Where(p => p.IdFact == factId).ToListAsync();
    }

    public async Task<ServiceResult> RecordPaymentAsync(
        int factId, int? commandeId, decimal amount, string mode, string? note, string userLogin)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        // ── Vérification période active ──
        var periode = await ctx.Periodes.FirstOrDefaultAsync(p => p.Activated == true);
        if (periode == null || periode.FromDate == null || periode.ToDate == null)
            return ServiceResult.Fail("Aucune période active n'est définie. Veuillez créer une période avant d'effectuer cette opération.");

        var today = DateTime.Today;
        if (today < periode.FromDate.Value.Date || today > periode.ToDate.Value.Date)
            return ServiceResult.Fail("La date du jour n'est pas comprise dans la période active.");

        // ── Vérification clôture journalière ──
        var todayOnly = DateOnly.FromDateTime(today);
        var userLocIds = await ctx.UserLocalisations
            .Where(ul => ul.UserId == userLogin && ul.LocalisationId.HasValue)
            .Select(ul => ul.LocalisationId!.Value)
            .ToListAsync();
        var hasClosed = await ctx.Versements.AnyAsync(v =>
            v.UserLogin == userLogin
            && v.DateCloture == todayOnly
            && userLocIds.Contains(v.LocalisationId)
            && v.StatutCloture == 0);
        if (hasClosed)
            return ServiceResult.Fail("Clôture en attente d'approbation admin. Aucune transaction n'est possible.");

        await using var tx = await ctx.Database.BeginTransactionAsync();

        try
        {
            // 1) Create the payment
            var facture = await ctx.Facts.Include(f => f.Money).FirstOrDefaultAsync(f => f.Id == factId);
            var moneyId = facture?.MoneyId ?? CurrencyDefaults.MoneyIdCdf;
            var taux = facture?.Taux ?? 0;

            var payment = new Payment
            {
                IdFact = factId,
                Date = DateOnly.FromDateTime(DateTime.Today),
                Note = mode + (string.IsNullOrWhiteSpace(note) ? "" : $" — {note}"),
                DateSys = DateTime.Now,
                User = userLogin
            };
            CurrencyFormat.ApplySnapshot(payment, CurrencyFormat.FromNativeAmount(amount, moneyId, taux));
            ctx.Payments.Add(payment);
            await ctx.SaveChangesAsync();

            // 2) If commandeId is set, update Commande payment fields
            if (commandeId.HasValue)
            {
                var commande = await ctx.Commandes.FindAsync(commandeId.Value);
                if (commande != null)
                {
                    commande.MontantPaye += amount;
                    commande.MontantRest = commande.MontantTotal - commande.MontantPaye;
                    if (commande.MontantRest < 0) commande.MontantRest = 0;
                }
            }

            // 3) Recalculate facture status based on total payments
            if (facture != null)
            {
                var totalPaye = await ctx.Payments
                    .Where(p => p.IdFact == factId)
                    .SumAsync(p => p.Montant);

                var net = facture.Montant ?? facture.TotalApresReduction ?? facture.Total ?? 0;
                if (totalPaye >= net)
                    facture.Status = 2; // Fully paid
                else
                    facture.Status = 1; // Partial payment
            }

            await ctx.SaveChangesAsync();
            await tx.CommitAsync();

            return ServiceResult.Ok("Paiement enregistré avec succès.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return ServiceResult.Fail(ex.InnerException?.Message ?? ex.Message);
        }
    }
}
