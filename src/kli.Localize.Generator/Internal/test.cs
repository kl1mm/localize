using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Translations = System.Collections.Generic.Dictionary<string, string>;

namespace kli.Localize.Generator.Internal
{
    public class LocalizationProvider
    {
        internal string GetValue(string key, CultureInfo cultureInfo)
        {
            bool ValueSelector(Translations translations, out string value)
            {
                if (translations.TryGetValue(key, out value))
                    return true;

                value = key;
                return false;
            }

            return TraverseCultures<string>(cultureInfo, ValueSelector);
        }

        internal IDictionary<string, string> GetValues(CultureInfo cultureInfo)
        {
            bool ValueSelector(Translations translations, out Translations value)
            {
                value = translations;
                return true;
            }

            

            return TraverseCultures<Translations>(cultureInfo, ValueSelector);
        }

        private T TraverseCultures<T>(CultureInfo cultureInfo, SelectorFunc<T> selectorFunc)
        {
            while (cultureInfo != CultureInfo.InvariantCulture)
            {
                if (resources.TryGetValue(cultureInfo, out Translations translations))
                {
                    if (selectorFunc(translations, out T result))
                        return result;
                }
                cultureInfo = cultureInfo.Parent;
            }

            selectorFunc(resources[CultureInfo.InvariantCulture], out T retVal);
            return retVal;
        }

        delegate bool SelectorFunc<T>(Translations translations, out T arg);


        private static readonly Translations invariant = new()
        {
            { "TestKey", " Neutral " }, };
            //{ "OnlyHere", "" Nür hiär "" },
            //{
            //    "SpecialChars",
            //    "" \" \\ @ ß" " },};


        private static readonly Translations en_us = new()
        { { "TestKey", "US" }, };
        private static readonly Translations en = new()
        { { "TestKey", "EN" }, };
        private static readonly Dictionary<CultureInfo, Translations> resources = new()
        { { CultureInfo.InvariantCulture, invariant }, { new CultureInfo("en-US"), en_us }, { new CultureInfo("en"), en }, };
    }
}
