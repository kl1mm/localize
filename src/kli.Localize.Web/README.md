# Localize

Simple extension package for [**kli.Localize**](https://www.nuget.org/packages/kli.Localize/) to register and use generated localizations as IStringLocalizer for e.g asp net core web projects
```c#
    services.AddLocalizationFromGenerated(o =>
    {
        o.LocationsToSearchGeneratedLocales = new []{ typeof(Locale).Assembly};
        o.UseExistingLocalizationRegistrationsAsFallback = true; // if this is true existing resx Localizer still working
    });
```