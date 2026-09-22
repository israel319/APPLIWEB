using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Domain.Entities.Parametres;

/// <summary>
/// Modèle de facture (ticket caisse ou A4 avec en-tête / pied de page / filigrane).
/// Un seul modèle actif (<see cref="IsActive"/>) est utilisé à l'impression.
/// </summary>
[Table("T_InvoiceTemplate")]
public class InvoiceTemplate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nom { get; set; } = "Modèle par défaut";

    public bool IsActive { get; set; }

    /// <summary>Ticket80 ou A4Portrait — voir <see cref="Common.InvoicePrintFormat"/>.</summary>
    [Required]
    [MaxLength(20)]
    public string PrintFormat { get; set; } = InvoicePrintFormat.Ticket80;

    /// <summary>Logo en-tête (URL ou data URI). Si vide, repli sur T_Profile.PhotoShop.</summary>
    public string? Logo { get; set; }

    /// <summary>Image de fond / filigrane (logo centré en arrière-plan sur A4).</summary>
    public string? LetterheadBackground { get; set; }

    public bool ShowLetterheadBackground { get; set; }

    [MaxLength(120)]
    public string? LegalRccm { get; set; }

    [MaxLength(120)]
    public string? LegalIdNat { get; set; }

    [MaxLength(500)]
    public string? LegalAdresse { get; set; }

    [MaxLength(250)]
    public string? LegalContact { get; set; }

    [MaxLength(250)]
    public string? LegalEmail { get; set; }

    [MaxLength(250)]
    public string? LegalWebsite { get; set; }

    [MaxLength(500)]
    public string? FooterText { get; set; }

    /// <summary>Marge haute du corps (mm) — laisse place à l'en-tête A4.</summary>
    public int ContentMarginTopMm { get; set; } = 42;

    /// <summary>Marge basse du corps (mm) — laisse place au pied de page A4.</summary>
    public int ContentMarginBottomMm { get; set; } = 32;
}
