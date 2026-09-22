namespace AppPlusPlus.Domain.Common;

/// <summary>Formats d'impression des factures.</summary>
public static class InvoicePrintFormat
{
    public const string Ticket80 = "Ticket80";
    public const string A4Portrait = "A4Portrait";

    public static bool IsA4(string? format) =>
        string.Equals(format, A4Portrait, StringComparison.OrdinalIgnoreCase);
}
