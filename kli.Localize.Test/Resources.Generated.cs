//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by kli.Localize.Tool on 02.04.2020 08:23:54.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace kli.Localize.Test
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.IO;
    using System.Globalization;
    using System.Collections.Generic;
    using Translations = System.Collections.Generic.IDictionary<string, string>;

    public sealed class Resources
    {
        private static readonly LocalizationProvider provider = new LocalizationProvider();
        public static Translations GetAll() => provider.GetValues(CultureInfo.CurrentUICulture);
        public static string TestKey => provider.GetValue(nameof(TestKey), CultureInfo.CurrentUICulture);
        private class LocalizationProvider
        {
            private static readonly IDictionary<CultureInfo, Translations> resources = new Dictionary<CultureInfo, Translations>();
            internal string GetValue(string key, CultureInfo cultureInfo)
            {
                if (this.GetTranslations(cultureInfo).TryGetValue(key, out var value))
                    return value;
                return key;
            }

            internal Translations GetValues(CultureInfo cultureInfo) => this.GetTranslations(cultureInfo);
            private Translations GetTranslations(CultureInfo cultureInfo)
            {
                if (!resources.TryGetValue(cultureInfo, out var translations))
                {
                    translations = Load(cultureInfo);
                    resources.Add(cultureInfo, translations);
                }

                return translations;
            }

            private Translations Load(CultureInfo cultureInfo)
            {
                return LoadResources(cultureInfo).SelectMany(dict => dict).ToLookup(pair => pair.Key, pair => pair.Value).ToDictionary(group => group.Key, group => group.First());
            }

            private IEnumerable<Translations> LoadResources(CultureInfo cultureInfo)
            {
                while (cultureInfo != CultureInfo.InvariantCulture)
                {
                    yield return LoadResource(cultureInfo);
                    cultureInfo = cultureInfo.Parent;
                }

                yield return LoadResource(CultureInfo.InvariantCulture);
            }

            private Translations LoadResource(CultureInfo cultureInfo)
            {
                var resourceName = $"{typeof(Resources).FullName}.json";
                if (cultureInfo != CultureInfo.InvariantCulture)
                    resourceName = resourceName.Replace(".json", $"_{cultureInfo.Name}.json");
                var assembly = typeof(Resources).Assembly;
                if (assembly.GetManifestResourceNames().Any(n => n.Equals(resourceName)))
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                        using (StreamReader reader = new StreamReader(stream ?? Stream.Null))
                            return JsonSerializer.Deserialize<Translations>(reader.ReadToEnd());
                }

                return new Dictionary<string, string>();
            }
        }
    }
}