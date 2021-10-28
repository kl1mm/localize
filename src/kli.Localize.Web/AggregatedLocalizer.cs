using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace kli.Localize.Web
{
    internal class AggregatedLocalizer<T> : IStringLocalizer<T>
    {
        private readonly IServiceProvider _provider;

        private IEnumerable<IStringLocalizer<T>> OtherLocalizers => _provider.GetServices<IStringLocalizer<T>>().Where(l => l.GetType() != GetType());

        public AggregatedLocalizer(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return OtherLocalizers.SelectMany(l => l.GetAllStrings()).Distinct();
        }

        public LocalizedString this[string name]
        {
            get
            {
                return this.OtherLocalizers.Select(l => l[name]).FirstOrDefault(s => !s.ResourceNotFound) ??
                       this.OtherLocalizers.Select(l => l[name]).FirstOrDefault(s => !string.IsNullOrEmpty(s.Value));
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                return this.OtherLocalizers.Select(l => l[name, arguments]).FirstOrDefault(s => !s.ResourceNotFound) ??
                       this.OtherLocalizers.Select(l => l[name, arguments]).FirstOrDefault(s => !string.IsNullOrEmpty(s.Value));
            }
        }
    }
}