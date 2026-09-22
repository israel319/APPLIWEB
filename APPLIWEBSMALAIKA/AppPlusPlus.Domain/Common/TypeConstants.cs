namespace AppPlusPlus.Domain.Common;

public static class TypesMouvement
{
    public const string ENTREE = "ENTREE";
    public const string SORTIE = "SORTIE";
    public const string TRANSFERT = "TRANSFERT";
    public const string AJUSTEMENT = "AJUSTEMENT";
}

public static class TypesDocument
{
    public const string APPRO = "APPRO";
    public const string FACTURE = "FACTURE";
    public const string LIVRAISON = "LIVRAISON";
    public const string INVENTAIRE = "INVENTAIRE";
    public const string TRANSFERT = "TRANSFERT";
    public const string TRANSFORMATION = "TRANSFORMATION";
    public const string DEMANDE = "DEMANDE";
}

/// <summary>Statuts T_Appros — null/0 = réception comptabilisée, 1 = transfert interne, 3 = annulé.</summary>
public static class ApproStatus
{
    public const int Direct = 0;
    public const int Transfer = 1;
    public const int Cancelled = 3;
}
