namespace AppPlusPlus.Application.DTOs.Prestations;

public class ServiceProjectRowDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? CustomerName { get; set; }
    public int Status { get; set; }
    public int BillingMode { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? FlatAmount { get; set; }
    public bool FlatInvoiced { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal ApprovedHours { get; set; }
    public decimal BillableHours { get; set; }
    public int TaskCount { get; set; }
    public DateTime DateSys { get; set; }
    public string User { get; set; } = "";
}

public class ServiceTimesheetRowDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectTitle { get; set; } = "";
    public int? TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public string UserLogin { get; set; } = "";
    public DateOnly WorkDate { get; set; }
    public decimal Hours { get; set; }
    public string? Description { get; set; }
    public int Status { get; set; }
    public int? FactId { get; set; }
}

public class ServiceTaskRowDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectTitle { get; set; } = "";
    public string Title { get; set; } = "";
    public string? AssignedUser { get; set; }
    public DateOnly? PlannedStart { get; set; }
    public DateOnly? PlannedEnd { get; set; }
    public int Status { get; set; }
    public int Priority { get; set; }
    public decimal LoggedHours { get; set; }
}

public class ServiceDeliverableRowDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectTitle { get; set; } = "";
    public string Title { get; set; } = "";
    public DateOnly? DueDate { get; set; }
    public DateOnly? DeliveredDate { get; set; }
    public int Status { get; set; }
}

public class ServiceProjectDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int Status { get; set; }
    public int BillingMode { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? FlatAmount { get; set; }
    public bool FlatInvoiced { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? LocalisationId { get; set; }
    public int? MoneyId { get; set; }
    public decimal ApprovedHours { get; set; }
    public decimal BillableHours { get; set; }
}

public class ServiceInvoiceRowDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectTitle { get; set; } = "";
    public string ClientName { get; set; } = "";
    public DateOnly Date { get; set; }
    public decimal Total { get; set; }
    public int Status { get; set; }
    public string User { get; set; } = "";
}

public class ServiceInvoiceLineDto
{
    public string Label { get; set; } = "";
    public double Qte { get; set; }
    public double Pu { get; set; }
    public decimal? Montant { get; set; }
    public int? MoneyId { get; set; }
    public decimal? Taux { get; set; }
    public decimal? MontantApresConversion { get; set; }
    public decimal LineTotal => (decimal)(Qte * Pu);
}

public class ServiceInvoiceDetailDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectTitle { get; set; } = "";
    public string ClientName { get; set; } = "";
    public DateOnly Date { get; set; }
    public decimal Total { get; set; }
    public decimal? TotalApresReduction { get; set; }
    public decimal Taux { get; set; }
    public int? MoneyId { get; set; }
    public int Status { get; set; }
    public bool IsUsd { get; set; }
    public string? MontantEnLettres { get; set; }
    public List<ServiceInvoiceLineDto> Lines { get; set; } = new();
}
