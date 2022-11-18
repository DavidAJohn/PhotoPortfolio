namespace PhotoPortfolio.Client.Helpers;

public static class StringExtensions
{
    public static string Truncate(this string inputString, int length)
    {
        if (string.IsNullOrWhiteSpace(inputString)) return inputString;

        return inputString.Length <= length ? inputString : string.Concat(inputString.AsSpan(0, (length-3)), "...");
    }
}
