using AppPlusPlus.Application.Interfaces;

namespace AppPlusPlus.Infrastructure.ExternalServices;

public class NumberToWordsConverter : INumberToWordsConverter
{
    private static readonly string[] Unites =
    {
        "", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf",
        "dix", "onze", "douze", "treize", "quatorze", "quinze", "seize",
        "dix-sept", "dix-huit", "dix-neuf"
    };

    private static readonly string[] Dizaines =
    {
        "", "dix", "vingt", "trente", "quarante", "cinquante",
        "soixante", "soixante", "quatre-vingt", "quatre-vingt"
    };

    public string Convert(decimal montant, string devise = "francs", string centime = "centimes")
    {
        if (montant == 0) return $"zéro {devise}";

        var abs = Math.Abs(montant);
        var partieEntiere = (long)Math.Truncate(abs);
        var partieDecimale = (int)Math.Round((abs - partieEntiere) * 100);

        var result = "";

        if (partieEntiere > 0)
            result = ConvertirNombre(partieEntiere) + " " + devise;
        else
            result = $"zéro {devise}";

        if (partieDecimale > 0)
            result += " et " + ConvertirNombre(partieDecimale) + " " + centime;

        if (montant < 0)
            result = "moins " + result;

        return char.ToUpper(result[0]) + result[1..];
    }

    private static string ConvertirNombre(long n)
    {
        if (n == 0) return "zéro";
        if (n < 0) return "moins " + ConvertirNombre(-n);

        var parties = new List<string>();

        if (n >= 1_000_000_000_000)
        {
            var trillions = n / 1_000_000_000_000;
            parties.Add(trillions == 1 ? "un billion" : ConvertirNombre(trillions) + " billions");
            n %= 1_000_000_000_000;
        }

        if (n >= 1_000_000_000)
        {
            var milliards = n / 1_000_000_000;
            parties.Add(milliards == 1 ? "un milliard" : ConvertirNombre(milliards) + " milliards");
            n %= 1_000_000_000;
        }

        if (n >= 1_000_000)
        {
            var millions = n / 1_000_000;
            parties.Add(millions == 1 ? "un million" : ConvertirNombre(millions) + " millions");
            n %= 1_000_000;
        }

        if (n >= 1000)
        {
            var milliers = n / 1000;
            parties.Add(milliers == 1 ? "mille" : ConvertirMoinsQueMille(milliers) + " mille");
            n %= 1000;
        }

        if (n > 0)
            parties.Add(ConvertirMoinsQueMille(n));

        return string.Join(" ", parties).Trim();
    }

    private static string ConvertirMoinsQueMille(long n)
    {
        if (n >= 100)
        {
            var centaines = n / 100;
            var reste = n % 100;

            string result = centaines == 1 ? "cent" : Unites[centaines] + " cent";

            if (reste == 0 && centaines > 1)
                result += "s";
            else if (reste > 0)
                result += " " + ConvertirMoinsQueCent(reste);

            return result;
        }

        return ConvertirMoinsQueCent(n);
    }

    private static string ConvertirMoinsQueCent(long n)
    {
        if (n < 20)
            return Unites[n];

        var dizaine = n / 10;
        var unite = n % 10;

        return dizaine switch
        {
            7 => unite == 1 ? "soixante-et-onze" : "soixante-" + Unites[10 + unite],
            9 => "quatre-vingt-" + Unites[10 + unite],
            8 => unite == 0 ? "quatre-vingts" : "quatre-vingt-" + Unites[unite],
            _ => unite == 0 ? Dizaines[dizaine] :
                 unite == 1 && dizaine <= 6 ? Dizaines[dizaine] + "-et-un" :
                 Dizaines[dizaine] + "-" + Unites[unite]
        };
    }
}
