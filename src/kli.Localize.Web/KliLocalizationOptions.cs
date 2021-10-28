using System.Reflection;

namespace kli.Localize.Web
{
    public class KliLocalizationOptions
    {
        public bool UseExistingLocalizationRegistrationsAsFallback { get; set; } = true;
        public Assembly[] LocationsToSearchGeneratedLocales { get; set; }
    }
}