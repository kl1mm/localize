using System;
using System.Globalization;

namespace kli.Localize.Example;

class Program
{
    static void Main(string[] args)
    {
        // Neutral/Invariant
        Console.WriteLine(Localizations.MyLocale.MyText);  // Hallo Welt (German)
        Console.WriteLine(Localizations.MyLocale.Sub.Nested);  // Kind (German)

        CultureInfo.CurrentUICulture = new CultureInfo("en");
        Console.WriteLine(Localizations.MyLocale.MyText);  // Hello World (English)
        Console.WriteLine(Localizations.MyLocale.Sub.Nested);  // Child (English)

        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        Console.WriteLine(Localizations.MyLocale.MyText);  // Hello World (English) -- Fallback to Parent.Culture
        Console.WriteLine(Localizations.MyLocale.Sub.Nested);  // Child (English) -- Fallback to Parent.Culture

        CultureInfo.CurrentUICulture = new CultureInfo("fr");
        Console.WriteLine(Localizations.MyLocale.MyText);  // Hallo Welt (German) -- Fallback to Neutral
        Console.WriteLine(Localizations.MyLocale.Sub.Nested);  // Kind (German) -- Fallback to Neutral
    }
}