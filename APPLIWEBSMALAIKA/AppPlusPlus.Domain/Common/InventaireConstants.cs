namespace AppPlusPlus.Domain.Common;

public static class InventaireStatut
{
    public const int Brouillon = 0;
    public const int Ouvert = 1;
    public const int Cloture = 2;
}

public static class InventaireReceptionStatut
{
    public const int Ouvert = 0;
    public const int Cloture = 1;
}

public static class InventaireTypePeriode
{
    public const int Jour = 0;
    public const int Mois = 1;
    public const int Exercice = 2;
}
