namespace AppPlusPlus.Application.Interfaces;

public interface INumberToWordsConverter
{
    string Convert(decimal montant, string devise = "francs", string centime = "centimes");
}
