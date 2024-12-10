using System.Text.RegularExpressions;

namespace Tools.Extensions;

public static class StringExtensions
{
    public static string Capitalize(this string str)
    {
        return str[..1].ToUpper() + str[1..];
    }

    public static string RemoveAtSymbol(this string str)
    {
        return str.Replace("@", "");
    }
}