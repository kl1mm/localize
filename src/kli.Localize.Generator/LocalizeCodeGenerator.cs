using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kli.Localize.Generator.Internal;
using kli.Localize.Generator.Internal.Helper;
using kli.Localize.Generator.Internal.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace kli.Localize.Generator
{
    [Generator(LanguageNames.CSharp)]
    public class LocalizeCodeGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext ctx)
        {
            // #if DEBUG
            //             if (!System.Diagnostics.Debugger.IsAttached)
            //                 System.Diagnostics.Debugger.Launch();
            // #endif
            var assemblyNameProvider = ctx.CompilationProvider.Select(static (c, _) => c.AssemblyName);
            var configOptionsProvider = ctx.AnalyzerConfigOptionsProvider;
            var additionalTextsWithOptionsAndAssemblyName =
                ctx.AdditionalTextsProvider
                    .Where(static at => Path.GetExtension(at.Path) == ".json")
                    .Combine(configOptionsProvider)
                    .Where(static pair =>
                    {
                        var additionalText = pair.Left;
                        var options = pair.Right;
                        var isLocalizeFile = options.GetOptions(additionalText)
                            .TryGetValue($"build_metadata.AdditionalFiles.{NamesResolver.LocalizeGroupName}",
                                out var localizeFileMetaData) && localizeFileMetaData == "true";
                        return isLocalizeFile;
                    })
                    .Select(static (pair, _) =>
                    {
                        var additionalText = pair.Left;
                        var options = pair.Right;
                        var hasNeutralCulture = options.GetOptions(additionalText)
                            .TryGetValue($"build_metadata.{NamesResolver.LocalizePropertyName}.{NamesResolver.MetaDataNeutralCulture}",
                                out var neutralCulture);
                        ;
                        return (additionalText: pair.Left, hasNeutralCulture: hasNeutralCulture && !string.IsNullOrWhiteSpace(neutralCulture));
                    })
                    .Collect()
                    .Select(static (additionalTextsWithNeutralCultureInfo, _) =>
                    {
                        return additionalTextsWithNeutralCultureInfo.GroupBy(at =>
                            PathHelper.FileNameWithoutCulture(at.additionalText.Path));
                    })
                    .Combine(configOptionsProvider)
                    .Combine(assemblyNameProvider);
            
            ctx.RegisterSourceOutput(additionalTextsWithOptionsAndAssemblyName, (spc, pipeline) =>
            {
                var assemblyName = pipeline.Right;
                var optionsProvider = pipeline.Left.Right;
                foreach (var additionalTexts in pipeline.Left.Left)
                {
                    if (!additionalTexts.First().hasNeutralCulture)
                    {
                        var foo = optionsProvider.GetOptions(additionalTexts.First().additionalText).Keys;
                        DiagnosticsExtensions.ReportMissingNeutralCulture(spc.ReportDiagnostic, additionalTexts.First().additionalText);
                        continue;
                    }
                    var codeGenerator = new LocalizeCodeGeneratorCore();
                    var translationReader = new JsonTranslationReader(spc.ReportDiagnostic);
                    var namesResolver = new NamesResolver(additionalTexts.First().additionalText, assemblyName, optionsProvider);
                    var generatorData = new GeneratorDataBuilder(additionalTexts.Select(t => t.additionalText).ToList(), namesResolver, translationReader).Build();
                    spc.AddSource(generatorData.GeneratedFileName, codeGenerator.CreateClass(generatorData));
                }
            });
        }
    }
}
