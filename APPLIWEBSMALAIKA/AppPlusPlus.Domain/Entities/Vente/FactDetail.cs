using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Entities.Catalogue;
using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Domain.Entities.Vente;

/// <summary>
/// Correspond a la table [dbo].[T_Fact_Details] -- Lignes de detail d'une facture.
/// </summary>
[Table("T_Fact_Details")]
public class FactDetail : IMonetaryRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Id_Fact")]
    public int? IdFact { get; set; }

    [MaxLength(50)]
    [Column("Id_Article")]
    public string? IdArticle { get; set; }

    public double? Qte { get; set; }

    public double? Pu { get; set; }

    public int? Status { get; set; }

    public int? Localisationid { get; set; }

    /// <summary>Libellé prestation (ligne sans article stock).</summary>
    [MaxLength(200)]
    [Column("Description_Line")]
    public string? DescriptionLine { get; set; }

    /// <summary>FK vers T_Service_Timesheets si traçabilité directe.</summary>
    [Column("Id_Timesheet")]
    public int? TimesheetId { get; set; }

    /// <summary>Total ligne dans la devise de l'opération (figé à la sauvegarde).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Montant { get; set; }

    /// <summary>FK vers T_Moneys — devise de Montant.</summary>
    [Column("Id_Monais")]
    public int? MoneyId { get; set; }

    /// <summary>Taux FC/USD appliqué à la ligne (figé à la sauvegarde).</summary>
    [Column(TypeName = "decimal(18,0)")]
    public decimal? Taux { get; set; }

    /// <summary>Équivalent dans l'autre devise (FC si Montant en USD, USD si Montant en FC).</summary>
    [Column("Montant_Apres_Conversion", TypeName = "decimal(18,2)")]
    public decimal? MontantApresConversion { get; set; }

    // Navigation
    [ForeignKey(nameof(IdFact))]
    public Fact? Fact { get; set; }

    [ForeignKey(nameof(IdArticle))]
    public Article? Article { get; set; }

    [ForeignKey(nameof(MoneyId))]
    public Money? Money { get; set; }
}
