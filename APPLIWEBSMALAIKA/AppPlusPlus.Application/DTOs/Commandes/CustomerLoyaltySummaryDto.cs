namespace AppPlusPlus.Application.DTOs.Commandes;

public class CustomerLoyaltySummaryDto
{
    public int CustomerId { get; set; }
    public int AcquiredPoints { get; set; }
    public int PendingPoints { get; set; }
    public int QualifyingOrderCount { get; set; }
    public int PendingOrderCount { get; set; }
    public decimal TotalOrderedFc { get; set; }
    public DateTime? LastQualifyingOrderDate { get; set; }
}

public class CustomerLoyaltyOrderLineDto
{
    public int CommandeId { get; set; }
    public DateTime? CreationDate { get; set; }
    public int Status { get; set; }
    public decimal MontantFc { get; set; }
    public int Points { get; set; }
    public bool IsAcquired { get; set; }
    public string StatusLabel { get; set; } = "";
}
