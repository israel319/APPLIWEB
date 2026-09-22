using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Application.DTOs.Inventaire;

public class InventaireSessionDto
{
    public int IdInventaire { get; set; }
    public int IdLocalisation { get; set; }
    public string NomMagasin { get; set; } = "";
    public string Nom { get; set; } = "";
    public string? Description { get; set; }
    public int TypePeriode { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
    public int Statut { get; set; }
    public string StatutLabel { get; set; } = "";
    public decimal MontantInitial { get; set; }
    public decimal MontantReceptions { get; set; }
    public decimal MontantVentes { get; set; }
    public decimal MontantTheorique { get; set; }
    public int NbReceptionsOuvertes { get; set; }
    public int NbReceptionsCloturees { get; set; }
    public int NbVentes { get; set; }
    public DateTime DateCreation { get; set; }
    public string CreePar { get; set; } = "";
    public string? CloturePar { get; set; }
    public string ResponsableLabel =>
        UserDisplayNames.Format(!string.IsNullOrWhiteSpace(CloturePar) ? CloturePar : CreePar);

    /// <summary>Somme des dépenses saisies sur la session.</summary>
    public decimal TotalDepenses { get; set; }

    /// <summary>Lignes dépense + commentaire (historique / détail).</summary>
    public List<InventaireDepenseResumeDto> DepensesAvecCommentaire { get; set; } = new();
}

public class InventaireDepenseResumeDto
{
    public DateOnly Date { get; set; }
    public decimal Montant { get; set; }
    public string? Observation { get; set; }
}

public class InventaireReceptionDto
{
    public int IdReception { get; set; }
    public int IdInventaire { get; set; }
    public string SessionNom { get; set; } = "";
    public string NomMagasin { get; set; } = "";
    public DateOnly DateJour { get; set; }
    public string? NumeroReception { get; set; }
    public string? Reference { get; set; }
    public int Statut { get; set; }
    public string StatutLabel { get; set; } = "";
    public decimal MontantTotal { get; set; }
    public int NbLignes { get; set; }
    public DateTime? DerniereLigne { get; set; }
    public string? Observation { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateCloture { get; set; }
    public string CreePar { get; set; } = "";
    public string? CloturePar { get; set; }
    public string ResponsableLabel =>
        UserDisplayNames.Format(!string.IsNullOrWhiteSpace(CloturePar) ? CloturePar : CreePar);
    public List<InventaireReceptionLineDto> Lines { get; set; } = new();
}

public class InventaireReceptionLineDto
{
    public int Id { get; set; }
    public string IdArticle { get; set; } = "";
    public string ArticleName { get; set; } = "";
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal MontantLigne { get; set; }
    public DateTime DateLigne { get; set; }
    public DateOnly? DateExpiration { get; set; }
    public string? Observation { get; set; }
}

public class InventaireVenteDto
{
    public int Id { get; set; }
    public int IdInventaire { get; set; }
    public DateOnly DateVente { get; set; }
    public decimal Montant { get; set; }
    public decimal MontantVentes { get; set; }
    public decimal MontantDepenses { get; set; }
    public string? Observation { get; set; }
    public string CreePar { get; set; } = "";
    public string CreeParLabel => UserDisplayNames.Format(CreePar);
}

public class InventaireSessionCreateRequest
{
    public int IdLocalisation { get; set; }
    public string Nom { get; set; } = "";
    public string? Description { get; set; }
    public int TypePeriode { get; set; }
    public DateOnly DateDebut { get; set; }
    public decimal MontantInitial { get; set; }
    public string UserLogin { get; set; } = "";
}

public class InventaireReceptionSaveRequest
{
    public int? IdReception { get; set; }
    public int IdInventaire { get; set; }
    public DateOnly? DateJour { get; set; }
    public string? NumeroReception { get; set; }
    public string? Reference { get; set; }
    public string? Observation { get; set; }
    public string UserLogin { get; set; } = "";
    public List<InventaireReceptionLineInput> Lines { get; set; } = new();
}

public class InventaireReceptionLineInput
{
    public int? Id { get; set; }
    public string IdArticle { get; set; } = "";
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public DateTime? DateLigne { get; set; }
    public DateOnly? DateExpiration { get; set; }
    public string? Observation { get; set; }
}

public class InventaireVenteSaveRequest
{
    public int IdInventaire { get; set; }
    public DateOnly DateVente { get; set; }
    public decimal MontantVentes { get; set; }
    public decimal MontantDepenses { get; set; }
    public string? Observation { get; set; }
    public string UserLogin { get; set; } = "";
}

public class InventaireCloseDayRequest
{
    public int IdInventaire { get; set; }
    public DateOnly DateJour { get; set; }
    public decimal MontantVentes { get; set; }
    public decimal MontantDepenses { get; set; }
    public string? Observation { get; set; }
    public string UserLogin { get; set; } = "";
}
