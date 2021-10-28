using System.Collections.Generic;
using System.Linq;
using kli.Localize.Generator.Base;
using Microsoft.Extensions.Localization;

namespace kli.Localize.Web
{
    public class FromGeneratedLocalizer<T, TLocalization> : IStringLocalizer<T> 
        where TLocalization : GeneratedLocalizationBase<TLocalization>, new()
    {
        private GeneratedLocalizationBase Instance => GeneratedLocalizationBase<TLocalization>.GetInstance();
        private IDictionary<string, string> GetAll(bool includeParentCultures) => this.Instance.GetAll(includeParentCultures) ?? new Dictionary<string, string>();

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return this.GetAll(includeParentCultures).Select(pair => new LocalizedString(pair.Key, pair.Value));
        }
        
        public LocalizedString this[string name] => this[name, null];

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var result = Get(name);
                return new LocalizedString(name, arguments?.Any() == true ? string.Format(result.Value ?? string.Empty, arguments) : result.Value, !result.Found);
            }
        }

        private (bool Found, string Value) Get(string name)
        {
            var found = GetAll(false).ContainsKey(name);
            return (found, Instance.GetString(name));
        }
    }

}