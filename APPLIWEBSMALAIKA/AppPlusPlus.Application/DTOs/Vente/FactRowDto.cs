namespace AppPlusPlus.Application.DTOs.Vente;

/// <summary>
/// Flat row used to display factures in grids (VenteHub tab 0).
/// </summary>
public class FactRowDto
{
    public int Id { get; set; }
    public string Client { get; set; } = "";
    public string Articles { get; set; } = "";
    public double TotalQte { get; set; }
    public double Total { get; set; }
    public double Paye { get; set; }
    public double Solde { get; set; }
    public DateOnly Date { get; set; }
    public int Status { get; set; }
    public string Login { get; set; } = "";
    public decimal Taux { get; set; }

    /// <summary>Facture incluse dans une clôture approuvée.</summary>
    public bool EstCloturee { get; set; }

    /// <summary>Facture créée après la dernière clôture approuvée du jour.</summary>
    public bool ApresCloture { get; set; }

    /// <summary>Une clôture est en attente d'approbation sur cette date.</summary>
    public bool ClotureEnAttente { get; set; }

    /// <summary>
    /// Computed display status: 0=Brouillon, 1=Validee, 2=Payee, 3=Annulee, 4=Partiel.
    /// Honours explicit Status 2/3 from DB (legacy prod remap) before payment-based inference.
    /// </summary>
    public int DisplayStatus
    {
        get
        {
            if (Status == 0) return 0;
            if (Status == 3) return 3;
            if (Status == 2) return 2;
            if (Solde <= 0) return 2;
            if (Paye > 0) return 4;
            return 1;
        }
    }
}
