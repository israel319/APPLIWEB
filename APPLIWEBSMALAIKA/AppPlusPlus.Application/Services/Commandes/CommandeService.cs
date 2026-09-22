using AppPlusPlus.Application.DTOs.Commandes;
using AppPlusPlus.Domain.Entities.Commandes;
using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Application.Services.Commandes;

public class CommandeService : ICommandeService
{
    private readonly ICommandeRepository _commandeRepo;

    public CommandeService(ICommandeRepository commandeRepo)
    {
        _commandeRepo = commandeRepo;
    }

    public async Task<CommandeListResult> GetCommandesWithStatusUpdateAsync()
    {
        // 1. Load all commandes with Customer, Details, Livraisons, Factures
        var commandes = await _commandeRepo.GetAllWithFullDetailsAsync();

        // 2. Compute delivered quantities per CommandeDetail
        var allDetailIds = commandes
            .SelectMany(c => c.Details)
            .Select(d => d.Id)
            .ToList();

        var deliveredQty = await _commandeRepo.GetDeliveredQtyByDetailIdsAsync(allDetailIds);

        // 3. Compute paid amounts per Commande
        var cmdIds = commandes.Select(c => c.CommandeId).ToList();
        var paidByCommande = await _commandeRepo.GetPaidAmountByCommandeIdsAsync(cmdIds);

        // 4. Auto-correct payment amounts and statuses
        var commandesToUpdate = new List<Commande>();
        foreach (var cmd in commandes)
        {
            bool changed = false;

            // Reconcile payment amounts from Factures/Payments
            var paye = paidByCommande.GetValueOrDefault(cmd.CommandeId);
            if (cmd.MontantPaye != paye)
            {
                cmd.MontantPaye = paye;
                cmd.MontantRest = cmd.MontantTotal - paye;
                if (cmd.MontantRest < 0) cmd.MontantRest = 0;
                changed = true;
            }

            // Derive expected status from livraisons and payments
            bool hasLivraisons = cmd.Livraisons.Any();
            bool allDelivered = cmd.Details.Any() && hasLivraisons
                                && !HasRemainingToDeliver(cmd, deliveredQty);

            int expectedStatus;
            if (cmd.MontantPaye >= cmd.MontantTotal && cmd.MontantTotal > 0)
                expectedStatus = 3; // Fully paid (facturee)
            else if (allDelivered)
                expectedStatus = 2; // Fully delivered (livree)
            else if (hasLivraisons)
                expectedStatus = 1; // Partially delivered (en cours)
            else
                expectedStatus = 0; // New

            if (cmd.Status != expectedStatus)
            {
                cmd.Status = expectedStatus;
                changed = true;
            }

            if (changed)
                commandesToUpdate.Add(cmd);
        }

        // 5. Persist corrections
        if (commandesToUpdate.Any())
        {
            await _commandeRepo.UpdateCommandeStatusBatchAsync(commandesToUpdate);
        }

        return new CommandeListResult
        {
            Commandes = commandes,
            DeliveredQtyByDetailId = deliveredQty
        };
    }

    public async Task<List<Commande>> GetCommandesWithDetailsAsync()
    {
        return await _commandeRepo.GetAllWithFullDetailsAsync();
    }

    public async Task DeleteCommandeAsync(int commandeId)
    {
        var commande = await _commandeRepo.GetByIdAsync(commandeId);
        if (commande != null)
            await _commandeRepo.DeleteAsync(commande);
    }

    public bool HasRemainingToDeliver(Commande cmd, Dictionary<int, decimal> deliveredQty)
    {
        if (!cmd.Details.Any()) return false;
        foreach (var d in cmd.Details)
        {
            var livree = deliveredQty.GetValueOrDefault(d.Id);
            if ((d.Qte ?? 0) - livree > 0) return true;
        }
        return false;
    }

    public async Task<Commande?> GetCommandeWithDetailsAsync(int commandeId)
    {
        return await _commandeRepo.GetWithDetailsAsync(commandeId);
    }

    public async Task SaveCommandeAsync(Commande commande, List<CommandeDetail> details)
    {
        await _commandeRepo.SaveCommandeWithDetailsAsync(commande, details);
    }

    public async Task<CommandeDetailViewDto?> GetCommandeDetailViewAsync(int commandeId)
    {
        var cmd = await _commandeRepo.GetWithAllNavigationsAsync(commandeId);
        if (cmd is null)
            return null;

        // Compute delivered quantities
        var detailIds = cmd.Details.Select(d => d.Id).ToList();
        var deliveredQty = detailIds.Any()
            ? await _commandeRepo.GetDeliveredQtyByDetailIdsAsync(detailIds)
            : new Dictionary<int, decimal>();

        // Map article rows — prix recalculés depuis le catalogue (source de vérité)
        var taux = MonetaryStandard.ResolveTaux(cmd.Taux);
        var articleRows = cmd.Details.Select(d =>
        {
            var qte = d.Qte ?? 0;
            var livrQte = deliveredQty.GetValueOrDefault(d.Id);
            var quote = CommandePricing.QuoteDetailLine(
                qte,
                d.Pu,
                d.MoneyId,
                d.Article != null ? (decimal?)d.Article.Price : null,
                d.Article?.IdMonais,
                taux);

            return new CommandeArticleRow
            {
                ArticleName = d.Article?.Description ?? d.ArticleId ?? "—",
                Qte = qte,
                PU = quote.PuFc,
                Montant = quote.LineFc,
                QteLivree = livrQte
            };
        }).ToList();

        var displayTotalFc = articleRows.Sum(r => r.Montant);
        var displayPayeFc = cmd.MontantPaye;
        var displayRestFc = Math.Max(0, displayTotalFc - displayPayeFc);

        // Map livraison rows
        var livraisonRows = cmd.Livraisons
            .OrderByDescending(l => l.Date)
            .Select(l =>
            {
                var fact = l.Fact;
                var montant = l.Details.Sum(d => d.MontantPaye ?? 0);
                int? factureId = fact?.Id;
                decimal totalPaye = fact?.Payments.Sum(p => p.Montant) ?? 0;

                string statusLabel;
                string statusStyle;

                if (fact != null)
                {
                    if (totalPaye >= montant && montant > 0)
                    { statusLabel = "Payée"; statusStyle = "bg-success"; }
                    else if (totalPaye > 0)
                    { statusLabel = "Partiel"; statusStyle = "bg-warning text-dark"; }
                    else
                    { statusLabel = "Non payée"; statusStyle = "bg-danger"; }
                }
                else
                {
                    var s = l.Status ?? 0;
                    (statusLabel, statusStyle) = s switch
                    {
                        0 => ("En attente", "bg-light text-dark"),
                        1 => ("Livrée — Non facturée", "bg-info"),
                        _ => ("—", "bg-secondary")
                    };
                }

                return new CommandeLivraisonRow
                {
                    LivraisonId = l.LivraisonId,
                    Porteur = l.Porteur ?? "—",
                    Montant = montant,
                    Date = l.Date,
                    FactureId = factureId,
                    TotalPaye = totalPaye,
                    StatusLabel = statusLabel,
                    StatusStyle = statusStyle
                };
            }).ToList();

        return new CommandeDetailViewDto
        {
            Commande = cmd,
            ArticleRows = articleRows,
            LivraisonRows = livraisonRows,
            DisplayTotalFc = displayTotalFc,
            DisplayMontantPayeFc = displayPayeFc,
            DisplayMontantRestFc = displayRestFc
        };
    }

    public async Task<List<ClientCommandeSummaryDto>> GetClientCommandesAsync(int customerId)
    {
        var commandes = await _commandeRepo.GetByCustomerWithFullDetailsAsync(customerId);
        if (!commandes.Any())
            return new List<ClientCommandeSummaryDto>();

        var allDetailIds = commandes
            .SelectMany(c => c.Details)
            .Select(d => d.Id)
            .ToList();

        var deliveredQty = await _commandeRepo.GetDeliveredQtyByDetailIdsAsync(allDetailIds);
        var cmdIds = commandes.Select(c => c.CommandeId).ToList();
        var paidByCommande = await _commandeRepo.GetPaidAmountByCommandeIdsAsync(cmdIds);

        var commandesToUpdate = new List<Commande>();
        foreach (var cmd in commandes)
        {
            if (TryReconcileCommandeStatus(cmd, deliveredQty, paidByCommande))
                commandesToUpdate.Add(cmd);
        }

        if (commandesToUpdate.Any())
            await _commandeRepo.UpdateCommandeStatusBatchAsync(commandesToUpdate);

        return commandes.Select(cmd => MapClientSummary(cmd, deliveredQty)).ToList();
    }

    bool TryReconcileCommandeStatus(
        Commande cmd,
        Dictionary<int, decimal> deliveredQty,
        Dictionary<int, decimal> paidByCommande)
    {
        var changed = false;

        var paye = paidByCommande.GetValueOrDefault(cmd.CommandeId);
        if (cmd.MontantPaye != paye)
        {
            cmd.MontantPaye = paye;
            cmd.MontantRest = cmd.MontantTotal - paye;
            if (cmd.MontantRest < 0) cmd.MontantRest = 0;
            changed = true;
        }

        var hasLivraisons = cmd.Livraisons.Any();
        var allDelivered = cmd.Details.Any() && hasLivraisons
                           && !HasRemainingToDeliver(cmd, deliveredQty);

        int expectedStatus;
        if (cmd.MontantPaye >= cmd.MontantTotal && cmd.MontantTotal > 0)
            expectedStatus = 3;
        else if (allDelivered)
            expectedStatus = 2;
        else if (hasLivraisons)
            expectedStatus = 1;
        else
            expectedStatus = 0;

        if (cmd.Status != expectedStatus)
        {
            cmd.Status = expectedStatus;
            changed = true;
        }

        return changed;
    }

    static ClientCommandeSummaryDto MapClientSummary(Commande cmd, Dictionary<int, decimal> deliveredQty)
    {
        var taux = MonetaryStandard.ResolveTaux(cmd.Taux);

        var lines = cmd.Details.Select(d =>
        {
            var qte = d.Qte ?? 0;
            var livree = deliveredQty.GetValueOrDefault(d.Id);
            var quote = CommandePricing.QuoteDetailLine(
                qte,
                d.Pu,
                d.MoneyId,
                d.Article != null ? (decimal?)d.Article.Price : null,
                d.Article?.IdMonais,
                taux);

            return new ClientCommandeLineDto
            {
                ArticleName = d.Article?.Description ?? d.ArticleId ?? "—",
                Qte = qte,
                QteLivree = livree,
                LineTotalFc = quote.LineFc
            };
        }).ToList();

        var totalOrdered = lines.Sum(l => l.Qte);
        var totalDelivered = lines.Sum(l => l.QteLivree);
        var progress = totalOrdered > 0
            ? (int)Math.Min(100, Math.Round(totalDelivered / totalOrdered * 100, MidpointRounding.AwayFromZero))
            : 0;

        var montantFc = lines.Sum(l => l.LineTotalFc);

        return new ClientCommandeSummaryDto
        {
            CommandeId = cmd.CommandeId,
            CreationDate = cmd.CreationDate,
            Description = cmd.CommandeDescription,
            Status = cmd.Status ?? 0,
            MontantTotalFc = montantFc,
            MontantPaye = cmd.MontantPaye,
            MontantRest = Math.Max(0, montantFc - cmd.MontantPaye),
            TotalQtyOrdered = totalOrdered,
            TotalQtyDelivered = totalDelivered,
            DeliveryProgressPercent = progress,
            Lines = lines,
            Livraisons = cmd.Livraisons
                .OrderByDescending(l => l.Date)
                .Select(l => new ClientLivraisonSummaryDto
                {
                    LivraisonId = l.LivraisonId,
                    Date = l.Date,
                    Porteur = l.Porteur ?? "—",
                    StatusLabel = (l.Status ?? 0) switch
                    {
                        1 => "Livrée",
                        _ => "En attente"
                    }
                })
                .ToList()
        };
    }

    public async Task<Dictionary<int, CustomerLoyaltySummaryDto>> GetCustomerLoyaltySummariesAsync()
    {
        var commandes = await _commandeRepo.GetForLoyaltyCalculationAsync();
        return commandes
            .Where(c => c.CustomerId.HasValue)
            .GroupBy(c => c.CustomerId!.Value)
            .ToDictionary(g => g.Key, g => BuildLoyaltySummary(g.Key, g));
    }

    public async Task<List<CustomerLoyaltyOrderLineDto>> GetCustomerLoyaltyOrdersAsync(int customerId)
    {
        var commandes = await _commandeRepo.GetForLoyaltyCalculationAsync();
        return commandes
            .Where(c => c.CustomerId == customerId)
            .Select(MapLoyaltyOrderLine)
            .Where(l => l.Points > 0)
            .OrderByDescending(l => l.CreationDate)
            .ToList();
    }

    static CustomerLoyaltySummaryDto BuildLoyaltySummary(int customerId, IEnumerable<Commande> commandes)
    {
        var summary = new CustomerLoyaltySummaryDto { CustomerId = customerId };

        foreach (var cmd in commandes)
        {
            var fc = GetCommandeMontantFc(cmd);
            var points = CustomerLoyaltyRules.PointsFromAmount(fc);
            if (points <= 0)
                continue;

            if (CustomerLoyaltyRules.IsAcquiredStatus(cmd.Status))
            {
                summary.AcquiredPoints += points;
                summary.QualifyingOrderCount++;
                summary.TotalOrderedFc += fc;
                if (cmd.CreationDate > summary.LastQualifyingOrderDate)
                    summary.LastQualifyingOrderDate = cmd.CreationDate;
            }
            else if (CustomerLoyaltyRules.IsPendingStatus(cmd.Status))
            {
                summary.PendingPoints += points;
                summary.PendingOrderCount++;
            }
        }

        return summary;
    }

    static CustomerLoyaltyOrderLineDto MapLoyaltyOrderLine(Commande cmd)
    {
        var fc = GetCommandeMontantFc(cmd);
        var points = CustomerLoyaltyRules.PointsFromAmount(fc);
        var acquired = CustomerLoyaltyRules.IsAcquiredStatus(cmd.Status);

        return new CustomerLoyaltyOrderLineDto
        {
            CommandeId = cmd.CommandeId,
            CreationDate = cmd.CreationDate,
            Status = cmd.Status ?? 0,
            MontantFc = fc,
            Points = points,
            IsAcquired = acquired,
            StatusLabel = acquired
                ? "Points acquis"
                : CustomerLoyaltyRules.IsPendingStatus(cmd.Status)
                    ? "Points en attente"
                    : "—"
        };
    }

    static decimal GetCommandeMontantFc(Commande cmd)
    {
        if (cmd.Details.Any(d => d.Article != null))
            return CommandePricing.ResolveCommandeTotalFc(cmd);

        var taux = MonetaryStandard.ResolveTaux(cmd.Taux);
        var isUsd = CurrencyDefaults.IsUsdMoneyId(cmd.MoneyId ?? CurrencyDefaults.MoneyIdUsd);
        if (CurrencyDefaults.IsCdfMoneyId(cmd.MoneyId ?? CurrencyDefaults.MoneyIdUsd))
            return cmd.MontantTotal;

        return cmd.MontantApresConversion
               ?? CurrencyFormat.ToCdf(cmd.MontantTotal, isUsd, taux);
    }
}
