using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kli.Localize.Generator.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace kli.Localize.Web
{
    public static class ServiceCollectionExtensions
    {
        private static KliLocalizationOptions Configure(Action<KliLocalizationOptions> setupAction)
        {
            var options = new KliLocalizationOptions();
            setupAction(options);
            options.LocationsToSearchGeneratedLocales ??= new[] { Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly(), Assembly.GetExecutingAssembly() }.Distinct().ToArray();
            return options;
        }

        public static IServiceCollection AddLocalizationFromGenerated<TLocale>(this IServiceCollection services, Action<KliLocalizationOptions> setupAction)
        {
            var options = Configure(setupAction);
            return services.AddGeneratedLocale(typeof(TLocale))
                .TryRemoveOtherLocalizers(options)
                .TryAddAggregatedLocalizer();
        }

        public static IServiceCollection AddLocalizationFromGenerated(this IServiceCollection services, Action<KliLocalizationOptions> setupAction)
        {
            var options = Configure(setupAction);
            return services.AddGeneratedLocales(options)
                .TryRemoveOtherLocalizers(options)
                .TryAddAggregatedLocalizer();
        }

        private static IServiceCollection AddGeneratedLocales(this IServiceCollection services, KliLocalizationOptions options)
        {
            var generatedLocaleTypes = FindGeneratedLocales(options.LocationsToSearchGeneratedLocales);

            foreach (var localeType in generatedLocaleTypes)
                services.AddGeneratedLocale(localeType);
            
            return services;
        }

        private static IServiceCollection AddGeneratedLocale(this IServiceCollection services, Type localeType)
        {
            services.AddTransient(typeof(IGeneratedLocalization), localeType);
            services.AddTransient(localeType);
            services.AddTransient(typeof(IStringLocalizer<>), provider => new FromGeneratedLocalizer(provider.GetService(localeType) as IGeneratedLocalization));
            return services;
        }

        private static IServiceCollection TryAddAggregatedLocalizer(this IServiceCollection services)
        {
            if (services.All(d => d.ImplementationType != typeof(AggregatedLocalizer<>)))
                services.Add(ServiceDescriptor.Transient(typeof(IStringLocalizer<>), typeof(AggregatedLocalizer<>)));
            return services;
        }
        
        private static IServiceCollection TryRemoveOtherLocalizers(this IServiceCollection services, KliLocalizationOptions options)
        {
            if (!options.UseExistingLocalizationRegistrationsAsFallback)
            {
                foreach (var descriptor in services.Where(d => d.ServiceType == typeof(IStringLocalizer<>) && d.ImplementationType?.IsAssignableTo(typeof(FromGeneratedLocalizer<>)) != true))
                    services.Remove(descriptor);
            }

            return services;
        }

        private static IEnumerable<Type> FindGeneratedLocales(Assembly[] assemblies)
        {
            return assemblies.SelectMany(assembly =>
                assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IGeneratedLocalization)) && !t.IsAbstract)).Distinct();
        }
    }
}