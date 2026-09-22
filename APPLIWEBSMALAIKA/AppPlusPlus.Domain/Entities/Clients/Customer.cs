using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppPlusPlus.Domain.Entities.Clients;

[Table("T_Customer")]
public class Customer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CustomerId { get; set; }

    [MaxLength(50)]
    [Column("CustomerName")]
    public string? CustomerName { get; set; }

    [MaxLength(50)]
    public string? Contact { get; set; }

    [MaxLength(50)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Adress { get; set; }

    /// <summary>True = client permanent (avec compte), False = client passager</summary>
    public bool? IsPermanent { get; set; }

    /// <summary>FK vers T_Customer_Type</summary>
    public int? CustomerTypeId { get; set; }

    [ForeignKey(nameof(CustomerTypeId))]
    public CustomerType? CustomerType { get; set; }

    [MaxLength(50)]
    public string? UserLogin { get; set; }

    public DateOnly? CreationDate { get; set; }
}
