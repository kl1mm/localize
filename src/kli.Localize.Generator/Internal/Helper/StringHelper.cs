namespace kli.Localize.Generator.Internal.Helper;

internal static class StringHelper
{
    internal static string EscapeValue(string input)
    {
        return input
            .Replace("\\", "\\\\")  // Escape Backslashes
            .Replace("\"", "\\\"")  // Escape double quotes
            .Replace("\n", "\\n")   // Escape newline
            .Replace("\r", "\\r")   // Escape carriage return
            .Replace("\t", "\\t");  // Escape tab
    }
    
    internal static class Keys
    {
        private const string Separator = "::";
        internal static string NestedKey(string parentKey, string key) => $"{parentKey}{Separator}{key}";

        internal static string ReplaceSeparator(string nestedKey, string replacement) =>
            nestedKey.Replace(Separator, replacement);
    }
}