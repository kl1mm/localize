using System.Collections.Generic;
using System.Linq;
using kli.Localize.Generator.Base;
using Microsoft.Extensions.Localization;

namespace kli.Localize.Web
{
    public class FromGeneratedLocalizer<T> : IStringLocalizer<T>
    {
        private readonly IGeneratedLocalization _localizationBase;

        public FromGeneratedLocalizer(IGeneratedLocalization localizationBase)
        {
            this._localizationBase = localizationBase;
        }

        private IDictionary<string, string> GetAll(bool includeParentCultures) => this._localizationBase.GetAll(includeParentCultures) ?? new Dictionary<string, string>();

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
            return (found, _localizationBase.GetString(name));
        }
    }

    public class FromGeneratedLocalizer : FromGeneratedLocalizer<object>
    {
        public FromGeneratedLocalizer(IGeneratedLocalization localizationBase) : base(localizationBase)
        {}
    }

    public class FromGeneratedLocalizer<T, TLocalization> : FromGeneratedLocalizer<T> 
        where TLocalization : GeneratedLocalizationBase<TLocalization>, new()
    {
        public FromGeneratedLocalizer() : base(GeneratedLocalizationBase<TLocalization>.GetInstance())
        { }
    }

}