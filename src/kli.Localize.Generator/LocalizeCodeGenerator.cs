using System;
using System.Linq;
using kli.Localize.Generator.Internal;
using kli.Localize.Generator.Internal.Helper;
using kli.Localize.Generator.Internal.Json;
using Microsoft.CodeAnalysis;

namespace kli.Localize.Generator
{
    [Generator]
    public class LocalizeCodeGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // TODO: Run only if AdditionalFiles or one of the CultureFiles is changed
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // #if DEBUG
            //             if (!System.Diagnostics.Debugger.IsAttached)
            //                 System.Diagnostics.Debugger.Launch();
            // #endif

            var translationReader = new JsonTranslationReader(context.ReportDiagnostic);
            var codeGenerator = new LocalizeCodeGeneratorCore();
            var additionalFilesGroupedByFileNames = context.AdditionalFiles.Where(af => af.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).GroupBy(f => PathHelper.FileNameWithoutCulture(f.Path));
            foreach (var group in additionalFilesGroupedByFileNames)
            {
                var namesResolver = new NamesResolver(group.First(), context.Compilation.AssemblyName, context.AnalyzerConfigOptions);
                var ctx = new GeneratorDataBuilder(group.ToList(), namesResolver, translationReader).Build();
                context.AddSource(ctx.GeneratedFileName, codeGenerator.CreateClass(ctx));
            }
        }
    }
}
