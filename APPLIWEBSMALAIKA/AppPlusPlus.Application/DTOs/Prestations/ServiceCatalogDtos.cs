namespace AppPlusPlus.Application.DTOs.Prestations;

public class ServiceCatalogRowDto
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int MoneyId { get; set; }
    public string? MoneyLabel { get; set; }
    public bool IsUsd { get; set; }
    public int UnitType { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime DateSys { get; set; }
}

public class ServiceCatalogDetailDto
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Detail { get; set; }
    public decimal Price { get; set; }
    public int MoneyId { get; set; }
    public int UnitType { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
}
