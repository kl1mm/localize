using System;
using System.Globalization;

namespace kli.Localize.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // Neutral/Invariant
            Console.WriteLine(Localizations.MyLocale.MyText);  // Hallo Welt (German)

            CultureInfo.CurrentUICulture = new CultureInfo("en");
            Console.WriteLine(Localizations.MyLocale.MyText);  // Hello World (English)

            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            Console.WriteLine(Localizations.MyLocale.MyText);  // Hello World (English) -- Fallback to Parent.Culture

            CultureInfo.CurrentUICulture = new CultureInfo("fr");
            Console.WriteLine(Localizations.MyLocale.MyText);  // Hallo Welt (German) -- Fallback to Neutral
        }
    }
}
