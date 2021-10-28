using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Translations = System.Collections.Generic.Dictionary<string, string>;

namespace kli.Localize.Generator.Base
{
    public class GeneratedLocalizationBase<T> : GeneratedLocalizationBase
        where T : GeneratedLocalizationBase<T>, new()
    {
        private static readonly Lazy<T> current = new(() => new T());
        public static T GetInstance() => current.Value;
        public static IDictionary<string, string> GetAll(CultureInfo cultureInfo = null) => ((GeneratedLocalizationBase) GetInstance()).GetAll(cultureInfo);
        public static IDictionary<string, string> GetAll(bool includeParents, CultureInfo cultureInfo = null) => ((GeneratedLocalizationBase)GetInstance()).GetAll(includeParents, cultureInfo);
        public static string GetString(string key, CultureInfo cultureInfo = null) => ((GeneratedLocalizationBase)GetInstance()).GetString(key, cultureInfo);
        public static string GetString(string key, bool includeParents, CultureInfo cultureInfo = null) => ((GeneratedLocalizationBase)GetInstance()).GetString(key, includeParents, cultureInfo);
    }

    public class GeneratedLocalizationBase : IGeneratedLocalization
    {
        protected virtual LocalizationProviderBase Provider { get; }

        public IDictionary<string, string> GetAll(CultureInfo cultureInfo = null) => Provider.GetValues(cultureInfo);
        public IDictionary<string, string> GetAll(bool includeParents, CultureInfo cultureInfo = null) => includeParents ? Provider.GetValuesWithParents(cultureInfo) : Provider.GetValues(cultureInfo);
        public string GetString(string key, CultureInfo cultureInfo = null) => Provider.GetValue(key, cultureInfo);
        public string GetString(string key, bool includeParents, CultureInfo cultureInfo = null) => Provider.GetValue(key, cultureInfo, includeParents);

        protected class LocalizationProviderBase
        {
            protected virtual Dictionary<CultureInfo, Translations> resources { get; }

            protected delegate bool SelectorFunc<T>(Translations translations, out T arg);

            public string GetValue(string key, CultureInfo cultureInfo = null, bool includeParents = true)
            {
                cultureInfo ??= CultureInfo.CurrentUICulture;
                bool ValueSelector(Translations translations, out string value)
                {
                    if (translations.TryGetValue(key, out value))
                        return true;

                    value = key;
                    return false;
                }

                return TraverseCultures<string>(cultureInfo, ValueSelector, includeParents);
            }

            public IDictionary<string, string> GetValuesWithParents(CultureInfo cultureInfo = null)
            {
                cultureInfo ??= CultureInfo.CurrentUICulture;
                var res = new List<IDictionary<string, string>>();
                while (!Equals(cultureInfo, CultureInfo.InvariantCulture))
                {
                    res.Add(GetValues(cultureInfo));
                    cultureInfo = cultureInfo.Parent;
                }

                return res.SelectMany(dict => dict)
                    .ToLookup(pair => pair.Key, pair => pair.Value)
                    .ToDictionary(group => group.Key, group => group.First());
            }

            public IDictionary<string, string> GetValues(CultureInfo cultureInfo = null)
            {
                cultureInfo ??= CultureInfo.CurrentUICulture;
                bool ValueSelector(Translations translations, out Translations value)
                {
                    value = translations;
                    return true;
                }

                return TraverseCultures<Translations>(cultureInfo, ValueSelector);
            }

            protected T TraverseCultures<T>(CultureInfo cultureInfo, SelectorFunc<T> selectorFunc, bool includeParents = true)
            {
                if (resources.TryGetValue(cultureInfo, out Translations translations))
                {
                    if (selectorFunc(translations, out T result) || Equals(cultureInfo, CultureInfo.InvariantCulture))
                        return result;
                }

                return includeParents ? TraverseCultures<T>(cultureInfo.Parent, selectorFunc) : default;
            }
        }
    }
}