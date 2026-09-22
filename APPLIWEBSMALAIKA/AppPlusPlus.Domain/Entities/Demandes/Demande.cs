using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Administration;

namespace AppPlusPlus.Domain.Entities.Demandes;

[Table("T_Demandes")]
public class Demande
{
    [Key]
    [Column("Id_Demande")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdDemande { get; set; }

    public int Statut { get; set; } = DemandeStatut.EnAttenteAdmin;

    [Column("Id_Localisation_Demandeur")]
    public int IdLocalisationDemandeur { get; set; }

    [Column("Id_Localisation_Source")]
    public int IdLocalisationSource { get; set; }

    [MaxLength(255)]
    public string? Commentaire { get; set; }

    [Column("Commentaire_Admin")]
    [MaxLength(255)]
    public string? CommentaireAdmin { get; set; }

    [Column("Cree_Par")]
    [MaxLength(50)]
    public string CreePar { get; set; } = "SYSTEM";

    [Column("Date_Creation")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    [Column("Admin_Approuve_Par")]
    [MaxLength(50)]
    public string? AdminApprouvePar { get; set; }

    [Column("Date_Admin_Approuve")]
    public DateTime? DateAdminApprouve { get; set; }

    [Column("Agent_Approuve_Par")]
    [MaxLength(50)]
    public string? AgentApprouvePar { get; set; }

    [Column("Date_Agent_Approuve")]
    public DateTime? DateAgentApprouve { get; set; }

    [ForeignKey(nameof(IdLocalisationDemandeur))]
    public Localisation? LocalisationDemandeur { get; set; }

    [ForeignKey(nameof(IdLocalisationSource))]
    public Localisation? LocalisationSource { get; set; }

    public ICollection<DemandeDetail> Details { get; set; } = new List<DemandeDetail>();
}
