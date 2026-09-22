using System.Text;

namespace AppPlusPlus.Application.Common;

public static class ClientPhoneHelper
{
    public static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        var sb = new StringBuilder(phone.Length);
        foreach (var ch in phone.Trim())
        {
            if (char.IsDigit(ch))
                sb.Append(ch);
            else if (ch == '+' && sb.Length == 0)
                sb.Append(ch);
        }

        return sb.ToString();
    }
}
