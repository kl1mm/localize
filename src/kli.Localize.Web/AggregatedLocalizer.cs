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

        private IEnumerable<IStringLocalizer<T>> OtherLocalizers => _provider.GetServices<IStringLocalizer<T>>().Where(l => l.GetType() != GetType())
            .OrderBy(OrderKeySelector);

        public Func<IStringLocalizer<T>, string> OrderKeySelector { get; set; } = l => l.GetType().Name;

        public AggregatedLocalizer(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var result = new List<List<LocalizedString>>();
            foreach (var localizer in OtherLocalizers)
            {
                try
                {
                    // We need to enumerate completely here to ensure we can catch exception for blazor WASM 
                    result.Add(localizer.GetAllStrings(includeParentCultures).ToList());
                }
                catch
                { }
            }
            return result.SelectMany(l => l).Distinct();
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