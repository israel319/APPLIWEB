using AppPlusPlus.Application.DTOs.Demandes;
using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Application.Services.Localisation;
using AppPlusPlus.Application.Services.Stock;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Application.Services.Demandes;

public class DemandeService : IDemandeService
{
    private readonly IDemandeRepository _demandeRepo;
    private readonly IStockRepository _stockRepo;
    private readonly ILookupRepository _lookupRepo;
    private readonly ILocalisationService _localisationService;
    private readonly IStockTransferService _transferService;

    public DemandeService(
        IDemandeRepository demandeRepo,
        IStockRepository stockRepo,
        ILookupRepository lookupRepo,
        ILocalisationService localisationService,
        IStockTransferService transferService)
    {
        _demandeRepo = demandeRepo;
        _stockRepo = stockRepo;
        _lookupRepo = lookupRepo;
        _localisationService = localisationService;
        _transferService = transferService;
    }

    public async Task<List<DemandeRowDto>> GetDemandesAsync(DemandeUserContext user)
    {
        var locIds = user.HasGlobalScope
            ? new List<int>()
            : await _localisationService.GetUserLocalisationIdsAsync(user.Login, false);

        if (!user.HasGlobalScope && !locIds.Any())
            return new List<DemandeRowDto>();

        var scopeIds = user.HasGlobalScope ? new List<int>() : locIds;
        var demandes = await _demandeRepo.GetForUserAsync(user.HasGlobalScope, scopeIds);

        return demandes.Select(d => new DemandeRowDto
        {
            IdDemande = d.IdDemande,
            Statut = d.Statut,
            StatutLabel = GetStatutLabel(d.Statut),
            LocalisationDemandeur = d.LocalisationDemandeur?.DescriptionLocalisation,
            LocalisationSource = d.LocalisationSource?.DescriptionLocalisation,
            DateCreation = d.DateCreation,
            NbArticles = d.Details.Count,
            TotalQteDemandee = d.Details.Sum(dd => dd.QteDemandee),
            CanAdminApprove = user.CanApproveDepot && d.Statut == DemandeStatut.EnAttenteAdmin,
            CanAgentApprove = user.CanConfirmForLocalisation(d.IdLocalisationDemandeur)
                && d.Statut == DemandeStatut.EnAttenteAgent
        }).ToList();
    }

    public async Task<List<DemandeDetailLineDto>> GetDetailLinesAsync(int demandeId)
    {
        var demande = await _demandeRepo.GetByIdWithDetailsAsync(demandeId);
        if (demande == null) return new List<DemandeDetailLineDto>();

        var lines = new List<DemandeDetailLineDto>();
        foreach (var detail in demande.Details)
        {
            var depotStock = await _stockRepo.GetByArticleAndLocalisationAsync(
                detail.IdArticle, demande.IdLocalisationSource);

            var depotQte = depotStock?.Qte ?? 0;
            var depotSeuil = depotStock?.Seuil ?? 0;
            var maxApprovable = Math.Max(0, depotQte - depotSeuil);

            lines.Add(new DemandeDetailLineDto
            {
                DetailId = detail.Id,
                ArticleId = detail.IdArticle,
                ArticleDescription = detail.Article?.Description ?? detail.IdArticle,
                QteDemandee = detail.QteDemandee,
                QteApprouvee = detail.QteApprouvee ?? detail.QteDemandee,
                DepotQte = depotQte,
                DepotSeuil = depotSeuil,
                QteMaxApprovable = Math.Min(detail.QteDemandee, maxApprovable)
            });
        }

        return lines;
    }

    public async Task<DemandeActionResult> ApproveAdminAsync(
        DemandeUserContext user, int demandeId, Dictionary<int, decimal> qteParDetail, string? commentaire)
    {
        if (!user.CanApproveDepot)
            return Fail("Vous n'avez pas le droit d'approuver les demandes depuis le dépôt.");

        var demande = await _demandeRepo.GetByIdWithDetailsAsync(demandeId);
        if (demande == null)
            return Fail("Demande introuvable.");

        if (demande.Statut != DemandeStatut.EnAttenteAdmin)
            return Fail("Cette demande n'est pas en attente d'approbation dépôt.");

        var depot = await _lookupRepo.GetLocalisationByIdAsync(demande.IdLocalisationSource);
        if (depot == null || !depot.IsDepot)
            return Fail("La localisation source n'est pas un dépôt valide.");

        foreach (var detail in demande.Details)
        {
            if (!qteParDetail.TryGetValue(detail.Id, out var qte))
                qte = detail.QteDemandee;

            if (qte <= 0)
            {
                detail.QteApprouvee = 0;
                continue;
            }

            var depotStock = await _stockRepo.GetByArticleAndLocalisationAsync(
                detail.IdArticle, demande.IdLocalisationSource);

            var depotQte = depotStock?.Qte ?? 0;
            var depotSeuil = depotStock?.Seuil ?? 0;
            var maxApprovable = Math.Max(0, depotQte - depotSeuil);

            if (qte > maxApprovable)
            {
                var art = detail.Article?.Description ?? detail.IdArticle;
                return Fail(
                    $"Stock dépôt insuffisant pour « {art} » : max {maxApprovable:N0} (stock {depotQte:N0}, seuil {depotSeuil:N0}).");
            }

            if (qte > detail.QteDemandee)
                return Fail($"Quantité approuvée supérieure à la quantité demandée pour l'article {detail.IdArticle}.");

            detail.QteApprouvee = qte;
        }

        if (!demande.Details.Any(d => (d.QteApprouvee ?? 0) > 0))
            return Fail("Aucune ligne avec quantité approuvée > 0.");

        demande.Statut = DemandeStatut.EnAttenteAgent;
        demande.AdminApprouvePar = user.Login;
        demande.DateAdminApprouve = DateTime.Now;
        demande.CommentaireAdmin = commentaire;

        await _demandeRepo.UpdateAsync(demande);
        return Ok("Demande approuvée — en attente de confirmation magasin.");
    }

    public async Task<DemandeActionResult> RefuseAdminAsync(
        DemandeUserContext user, int demandeId, string? commentaire)
    {
        if (!user.CanApproveDepot)
            return Fail("Vous n'avez pas le droit de refuser les demandes dépôt.");

        var demande = await _demandeRepo.GetByIdWithDetailsAsync(demandeId);
        if (demande == null) return Fail("Demande introuvable.");
        if (demande.Statut != DemandeStatut.EnAttenteAdmin)
            return Fail("Cette demande n'est pas en attente d'approbation dépôt.");

        demande.Statut = DemandeStatut.RefuseeAdmin;
        demande.AdminApprouvePar = user.Login;
        demande.DateAdminApprouve = DateTime.Now;
        demande.CommentaireAdmin = commentaire;

        await _demandeRepo.UpdateAsync(demande);
        return Ok("Demande refusée.");
    }

    public async Task<DemandeActionResult> ApproveAgentAsync(DemandeUserContext user, int demandeId)
    {
        var demande = await _demandeRepo.GetByIdWithDetailsAsync(demandeId);
        if (demande == null) return Fail("Demande introuvable.");

        if (!user.CanConfirmForLocalisation(demande.IdLocalisationDemandeur))
            return Fail("Vous n'êtes pas autorisé à confirmer pour cette localisation.");

        if (demande.Statut != DemandeStatut.EnAttenteAgent)
            return Fail("Cette demande n'est pas en attente de confirmation magasin.");

        var lines = demande.Details
            .Where(d => (d.QteApprouvee ?? 0) > 0)
            .Select(d => new StockTransferLine(d.IdArticle, d.QteApprouvee!.Value))
            .ToList();

        if (!lines.Any())
            return Fail("Aucune quantité approuvée à transférer.");

        try
        {
            var count = await _transferService.TransferAsync(
                demande.IdLocalisationSource,
                demande.IdLocalisationDemandeur,
                lines,
                user.Login,
                $"Demande N°{demande.IdDemande}",
                demande.CommentaireAdmin ?? demande.Commentaire,
                TypesDocument.DEMANDE,
                demande.IdDemande);

            demande.Statut = DemandeStatut.Completee;
            demande.AgentApprouvePar = user.Login;
            demande.DateAgentApprouve = DateTime.Now;

            await _demandeRepo.UpdateAsync(demande);
            return Ok($"{count} article(s) transféré(s) depuis le dépôt.");
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public async Task<DemandeActionResult> RefuseAgentAsync(
        DemandeUserContext user, int demandeId, string? commentaire)
    {
        var demande = await _demandeRepo.GetByIdWithDetailsAsync(demandeId);
        if (demande == null) return Fail("Demande introuvable.");

        if (!user.CanConfirmForLocalisation(demande.IdLocalisationDemandeur))
            return Fail("Vous n'êtes pas autorisé à refuser pour cette localisation.");

        if (demande.Statut != DemandeStatut.EnAttenteAgent)
            return Fail("Cette demande n'est pas en attente de confirmation magasin.");

        demande.Statut = DemandeStatut.RefuseeAgent;
        demande.AgentApprouvePar = user.Login;
        demande.DateAgentApprouve = DateTime.Now;
        demande.Commentaire = commentaire;

        await _demandeRepo.UpdateAsync(demande);
        return Ok("Demande refusée par le magasin.");
    }

    private static string GetStatutLabel(int statut) => statut switch
    {
        DemandeStatut.EnAttenteAdmin => "Attente dépôt",
        DemandeStatut.EnAttenteAgent => "Attente magasin",
        DemandeStatut.Completee => "Complétée",
        DemandeStatut.RefuseeAdmin => "Refusée (dépôt)",
        DemandeStatut.RefuseeAgent => "Refusée (magasin)",
        DemandeStatut.Annulee => "Annulée",
        _ => "—"
    };

    private static DemandeActionResult Ok(string message) =>
        new() { Success = true, Message = message };

    private static DemandeActionResult Fail(string message) =>
        new() { Success = false, Message = message };
}
