using AppPlusPlus.Application.DTOs.Inventaire;
using AppPlusPlus.Application.Interfaces.Repositories;
using AppPlusPlus.Application.Services.Inventaire;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Inventaire;

namespace AppPlusPlus.Application.Services.Inventaire;

public class InventaireService : IInventaireService
{
    private readonly IInventaireRepository _repo;
    private readonly IInventairePostingService _posting;

    public InventaireService(IInventaireRepository repo, IInventairePostingService posting)
    {
        _repo = repo;
        _posting = posting;
    }

    public async Task<List<InventaireSessionDto>> GetSessionsAsync(IReadOnlyList<int> locIds, bool hasGlobalScope)
    {
        var sessions = await _repo.GetSessionsForLocalisationsAsync(locIds, hasGlobalScope);
        return sessions.Select(MapSession).ToList();
    }

    public async Task<InventaireSessionDto?> GetSessionAsync(int id)
    {
        var session = await _repo.GetSessionByIdAsync(id);
        return session == null ? null : MapSession(session);
    }

    public async Task<InventaireSessionDto?> GetOpenSessionForLocalisationAsync(int localisationId)
    {
        var session = await _repo.GetOpenSessionForLocalisationAsync(localisationId);
        return session == null ? null : MapSession(session);
    }

    public async Task<decimal> GetSuggestedMontantInitialAsync(int localisationId)
    {
        var last = await _repo.GetLastClosedSessionForLocalisationAsync(localisationId);
        return last?.MontantTheorique ?? 0;
    }

    public async Task<List<InventaireSessionDto>> GetSessionHistoryForLocalisationAsync(int localisationId)
    {
        var list = await _repo.GetClosedSessionsForLocalisationAsync(localisationId);
        return list.Select(MapSession).ToList();
    }

    public async Task<int> CreateSessionAsync(InventaireSessionCreateRequest request)
    {
        if (request.IdLocalisation <= 0)
            throw new InvalidOperationException("Sélectionnez un magasin.");

        if (request.MontantInitial <= 0)
        {
            var suggested = await GetSuggestedMontantInitialAsync(request.IdLocalisation);
            if (suggested <= 0)
                throw new InvalidOperationException("Saisissez la valeur de départ pour la première journée.");
            request.MontantInitial = suggested;
        }
        else if (request.MontantInitial < 0)
            throw new InvalidOperationException("Le montant initial doit être positif.");

        var existing = await _repo.GetOpenSessionForLocalisationAsync(request.IdLocalisation);
        if (existing != null)
            throw new InvalidOperationException(
                $"Une session est déjà ouverte pour ce magasin : « {existing.Nom} ».");

        var session = new InventaireMagasin
        {
            IdLocalisation = request.IdLocalisation,
            Nom = string.IsNullOrWhiteSpace(request.Nom)
                ? $"Inventaire {request.DateDebut:dd/MM/yyyy}"
                : request.Nom.Trim(),
            Description = request.Description?.Trim(),
            TypePeriode = request.TypePeriode,
            DateDebut = request.DateDebut,
            MontantInitial = request.MontantInitial,
            Statut = InventaireStatut.Ouvert,
            CreePar = TextFieldLimits.Truncate(request.UserLogin, TextFieldLimits.MouvementUser)
        };

        await _repo.AddSessionAsync(session);
        return session.IdInventaire;
    }

    public async Task UpdateMontantInitialAsync(int inventaireId, decimal montantInitial)
    {
        if (montantInitial < 0)
            throw new InvalidOperationException("Le montant doit être positif.");

        var session = await _repo.GetSessionByIdAsync(inventaireId)
                      ?? throw new InvalidOperationException("Session introuvable.");

        if (session.Statut == InventaireStatut.Cloture)
            throw new InvalidOperationException("Session clôturée.");

        session.MontantInitial = montantInitial;
        await _repo.UpdateSessionAsync(session);
    }

    public async Task CloseSessionAsync(int inventaireId, string userLogin)
    {
        var session = await _repo.GetSessionByIdAsync(inventaireId)
                      ?? throw new InvalidOperationException("Session introuvable.");

        if (session.Statut == InventaireStatut.Cloture)
            throw new InvalidOperationException("Session déjà clôturée.");

        var openReceptions = session.Receptions.Count(r => r.Statut == InventaireReceptionStatut.Ouvert);
        if (openReceptions > 0)
            throw new InvalidOperationException("Clôturez toutes les réceptions ouvertes avant de clôturer la session.");

        session.Statut = InventaireStatut.Cloture;
        session.DateCloture = DateTime.Now;
        session.CloturePar = TextFieldLimits.Truncate(userLogin, TextFieldLimits.MouvementUser);
        session.DateFin = DateOnly.FromDateTime(DateTime.Today);

        await _repo.UpdateSessionAsync(session);
    }

    public async Task<List<InventaireReceptionDto>> GetReceptionsAsync(int inventaireId)
    {
        var list = await _repo.GetReceptionsForSessionAsync(inventaireId);
        return list.Select(MapReceptionSummary).ToList();
    }

    public async Task<List<InventaireReceptionDto>> GetReceptionHistoryForLocalisationAsync(int localisationId)
    {
        var list = await _repo.GetReceptionHistoryForLocalisationAsync(localisationId);
        return list.Select(MapReceptionSummary).ToList();
    }

    public async Task<InventaireReceptionDto?> GetReceptionAsync(int receptionId)
    {
        var rec = await _repo.GetReceptionByIdWithDetailsAsync(receptionId);
        return rec == null ? null : MapReceptionFull(rec);
    }

    public async Task<InventaireReceptionDto?> GetReceptionForDayAsync(int inventaireId, DateOnly dateJour)
    {
        var rec = await _repo.GetReceptionForDayAsync(inventaireId, dateJour);
        return rec == null ? null : MapReceptionFull(rec);
    }

    public async Task<int> SaveReceptionAsync(InventaireReceptionSaveRequest request)
    {
        ValidateReceptionLines(request.Lines);

        var session = await _repo.GetSessionByIdAsync(request.IdInventaire)
                      ?? throw new InvalidOperationException("Session inventaire introuvable.");

        if (session.Statut == InventaireStatut.Cloture)
            throw new InvalidOperationException("Impossible de modifier une session clôturée.");

        var dateJour = request.DateJour ?? DateOnly.FromDateTime(DateTime.Today);
        InventaireReception reception;

        if (request.IdReception.HasValue)
        {
            reception = await _repo.GetReceptionByIdWithDetailsAsync(request.IdReception.Value)
                        ?? throw new InvalidOperationException("Réception introuvable.");

            if (reception.Statut == InventaireReceptionStatut.Cloture)
                throw new InvalidOperationException("Réception clôturée — impossible de modifier.");
        }
        else
        {
            var existing = await _repo.GetReceptionForDayAsync(request.IdInventaire, dateJour);
            if (existing != null)
            {
                if (existing.Statut == InventaireReceptionStatut.Cloture)
                    throw new InvalidOperationException($"La réception du {dateJour:dd/MM/yyyy} est clôturée.");

                reception = existing;
            }
            else
            {
                reception = new InventaireReception
                {
                    IdInventaire = request.IdInventaire,
                    DateJour = dateJour,
                    NumeroReception = $"REC-{dateJour:yyyyMMdd}",
                    CreePar = TextFieldLimits.Truncate(request.UserLogin, TextFieldLimits.MouvementUser)
                };
                await _repo.AddReceptionAsync(reception);
            }
        }

        reception.Reference = TextFieldLimits.Truncate(request.Reference, TextFieldLimits.MouvementReference);
        reception.Observation = TextFieldLimits.Truncate(request.Observation, TextFieldLimits.MouvementObservation);
        if (!string.IsNullOrWhiteSpace(request.NumeroReception))
            reception.NumeroReception = TextFieldLimits.Truncate(request.NumeroReception, 30);

        await _repo.UpdateReceptionAsync(reception);

        var details = request.Lines.Select(l => new InventaireReceptionDetail
        {
            IdArticle = l.IdArticle.Trim(),
            Quantite = QuantityFormat.Normalize(l.Quantite),
            PrixUnitaire = l.PrixUnitaire,
            DateLigne = l.DateLigne ?? dateJour.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)),
            DateExpiration = l.DateExpiration,
            Observation = TextFieldLimits.Truncate(l.Observation, TextFieldLimits.MouvementObservation)
        }).ToList();

        await _repo.SaveReceptionDetailsAsync(reception.IdReception, details);
        return reception.IdReception;
    }

    public Task CloseReceptionAsync(int receptionId, string userLogin)
        => _posting.CloseReceptionAsync(receptionId, userLogin);

    public async Task<List<InventaireVenteDto>> GetVentesAsync(int inventaireId)
    {
        var ventes = await _repo.GetVentesForSessionAsync(inventaireId);
        return ventes.Select(MapVente).ToList();
    }

    public async Task<InventaireVenteDto?> GetVenteForDayAsync(int inventaireId, DateOnly dateVente)
    {
        var v = await _repo.GetVenteForDayAsync(inventaireId, dateVente);
        return v == null ? null : MapVente(v);
    }

    public async Task SaveVenteAsync(InventaireVenteSaveRequest request)
    {
        if (request.MontantVentes < 0 || request.MontantDepenses < 0)
            throw new InvalidOperationException("Les montants doivent être positifs.");

        if (request.MontantDepenses > 0 && string.IsNullOrWhiteSpace(request.Observation))
            throw new InvalidOperationException("Une observation est obligatoire lorsqu'il y a des dépenses.");

        if (request.MontantVentes + request.MontantDepenses <= 0)
            throw new InvalidOperationException("Indiquez au moins un montant de vente ou de dépense.");

        var session = await _repo.GetSessionByIdAsync(request.IdInventaire)
                      ?? throw new InvalidOperationException("Session introuvable.");

        if (session.Statut == InventaireStatut.Cloture)
            throw new InvalidOperationException("Session clôturée.");

        await _repo.UpsertVenteAsync(new InventaireVente
        {
            IdInventaire = request.IdInventaire,
            DateVente = request.DateVente,
            MontantVentes = request.MontantVentes,
            MontantDepenses = request.MontantDepenses,
            Observation = TextFieldLimits.Truncate(request.Observation, TextFieldLimits.MouvementObservation),
            CreePar = TextFieldLimits.Truncate(request.UserLogin, TextFieldLimits.MouvementUser)
        });
    }

    public async Task CloseDayAsync(InventaireCloseDayRequest request)
    {
        if (request.MontantVentes < 0 || request.MontantDepenses < 0)
            throw new InvalidOperationException("Les montants doivent être positifs.");

        if (request.MontantDepenses > 0 && string.IsNullOrWhiteSpace(request.Observation))
            throw new InvalidOperationException("Une observation est obligatoire lorsqu'il y a des dépenses.");

        if (request.MontantVentes + request.MontantDepenses <= 0)
        {
            var recCheck = await _repo.GetReceptionForDayAsync(request.IdInventaire, request.DateJour);
            if (recCheck is null || recCheck.Details.Count == 0)
                throw new InvalidOperationException("Indiquez les ventes/dépenses ou ajoutez au moins une ligne de réception.");
        }

        var session = await _repo.GetSessionByIdAsync(request.IdInventaire)
                      ?? throw new InvalidOperationException("Session introuvable.");

        if (session.Statut == InventaireStatut.Cloture)
            throw new InvalidOperationException("Session déjà clôturée.");

        var reception = await _repo.GetReceptionForDayAsync(request.IdInventaire, request.DateJour);
        if (reception != null && reception.Statut == InventaireReceptionStatut.Ouvert)
        {
            if (reception.Details.Count == 0)
                throw new InvalidOperationException("La réception du jour n'a aucune ligne — ajoutez des articles ou supprimez-la.");

            await _posting.CloseReceptionAsync(reception.IdReception, request.UserLogin);
        }

        await _repo.UpsertVenteAsync(new InventaireVente
        {
            IdInventaire = request.IdInventaire,
            DateVente = request.DateJour,
            MontantVentes = request.MontantVentes,
            MontantDepenses = request.MontantDepenses,
            Observation = TextFieldLimits.Truncate(request.Observation, TextFieldLimits.MouvementObservation),
            CreePar = TextFieldLimits.Truncate(request.UserLogin, TextFieldLimits.MouvementUser)
        });

        if (session.TypePeriode == InventaireTypePeriode.Jour)
            await CloseSessionAsync(request.IdInventaire, request.UserLogin);
    }

    static InventaireVenteDto MapVente(InventaireVente v) => new()
    {
        Id = v.Id,
        IdInventaire = v.IdInventaire,
        DateVente = v.DateVente,
        Montant = v.Montant,
        MontantVentes = v.MontantVentes,
        MontantDepenses = v.MontantDepenses,
        Observation = v.Observation,
        CreePar = v.CreePar
    };

    public async Task DeleteVenteAsync(int venteId, int inventaireId)
    {
        var session = await _repo.GetSessionByIdAsync(inventaireId)
                      ?? throw new InvalidOperationException("Session introuvable.");

        if (session.Statut == InventaireStatut.Cloture)
            throw new InvalidOperationException("Session clôturée.");

        await _repo.DeleteVenteAsync(venteId);
    }

    static void ValidateReceptionLines(List<InventaireReceptionLineInput> lines)
    {
        if (lines.Count == 0)
            throw new InvalidOperationException("Ajoutez au moins un article.");

        if (lines.Any(l => string.IsNullOrWhiteSpace(l.IdArticle)))
            throw new InvalidOperationException("Tous les articles doivent être sélectionnés.");

        if (lines.Any(l => l.Quantite <= 0))
            throw new InvalidOperationException("Les quantités doivent être > 0.");

        if (lines.Any(l => l.PrixUnitaire < 0))
            throw new InvalidOperationException("Les prix unitaires doivent être positifs.");
    }

    static InventaireSessionDto MapSession(InventaireMagasin s)
    {
        var depenses = s.Ventes
            .Where(v => v.MontantDepenses > 0 || !string.IsNullOrWhiteSpace(v.Observation))
            .OrderByDescending(v => v.DateVente)
            .Select(v => new InventaireDepenseResumeDto
            {
                Date = v.DateVente,
                Montant = v.MontantDepenses,
                Observation = v.Observation
            })
            .ToList();

        return new InventaireSessionDto
        {
            IdInventaire = s.IdInventaire,
            IdLocalisation = s.IdLocalisation,
            NomMagasin = s.Localisation?.DescriptionLocalisation ?? $"Magasin #{s.IdLocalisation}",
            Nom = s.Nom,
            Description = s.Description,
            TypePeriode = s.TypePeriode,
            DateDebut = s.DateDebut,
            DateFin = s.DateFin,
            Statut = s.Statut,
            StatutLabel = SessionStatutLabel(s.Statut),
            MontantInitial = s.MontantInitial,
            MontantReceptions = s.MontantReceptions,
            MontantVentes = s.MontantVentes,
            MontantTheorique = s.MontantTheorique,
            NbReceptionsOuvertes = s.Receptions.Count(r => r.Statut == InventaireReceptionStatut.Ouvert),
            NbReceptionsCloturees = s.Receptions.Count(r => r.Statut == InventaireReceptionStatut.Cloture),
            NbVentes = s.Ventes.Count,
            DateCreation = s.DateCreation,
            CreePar = s.CreePar,
            CloturePar = s.CloturePar,
            TotalDepenses = depenses.Sum(d => d.Montant),
            DepensesAvecCommentaire = depenses
        };
    }

    static InventaireReceptionDto MapReceptionSummary(InventaireReception r) => new()
    {
        IdReception = r.IdReception,
        IdInventaire = r.IdInventaire,
        SessionNom = r.Inventaire?.Nom ?? "",
        NomMagasin = r.Inventaire?.Localisation?.DescriptionLocalisation ?? "",
        DateJour = r.DateJour,
        NumeroReception = r.NumeroReception,
        Reference = r.Reference,
        Statut = r.Statut,
        StatutLabel = r.Statut == InventaireReceptionStatut.Cloture ? "Clôturée" : "Ouverte",
        MontantTotal = r.MontantTotal,
        NbLignes = r.Details.Count,
        DerniereLigne = r.Details.OrderByDescending(d => d.DateLigne).FirstOrDefault()?.DateLigne,
        Observation = r.Observation,
        DateCreation = r.DateCreation,
        DateCloture = r.DateCloture,
        CreePar = r.CreePar,
        CloturePar = r.CloturePar
    };

    static InventaireReceptionDto MapReceptionFull(InventaireReception r)
    {
        var dto = MapReceptionSummary(r);
        dto.Lines = r.Details
            .OrderByDescending(d => d.DateLigne)
            .Select(d => new InventaireReceptionLineDto
        {
            Id = d.Id,
            IdArticle = d.IdArticle,
            ArticleName = d.Article?.Description ?? d.IdArticle,
            Quantite = d.Quantite,
            PrixUnitaire = d.PrixUnitaire,
            MontantLigne = d.MontantLigne,
            DateLigne = d.DateLigne,
            DateExpiration = d.DateExpiration,
            Observation = d.Observation
        }).ToList();
        return dto;
    }

    static string SessionStatutLabel(int statut) => statut switch
    {
        InventaireStatut.Brouillon => "Brouillon",
        InventaireStatut.Ouvert => "Ouvert",
        InventaireStatut.Cloture => "Clôturé",
        _ => "—"
    };
}
