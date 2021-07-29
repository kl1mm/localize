using kli.Localize.Generator.Internal;
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
//#if DEBUG
//            if (!System.Diagnostics.Debugger.IsAttached)
//                System.Diagnostics.Debugger.Launch();
//#endif

            var codeGenerator = new LocalizeCodeGeneratorCore(context.ReportDiagnostic);
            foreach (var file in context.AdditionalFiles)
            {
                var ctx = new GeneratorDataContext(context, file);
                context.AddSource(ctx.GeneratedFileName, codeGenerator.CreateClass(ctx));
            }
        }
    }
}
