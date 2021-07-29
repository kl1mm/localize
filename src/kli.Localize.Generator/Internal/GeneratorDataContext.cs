using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace kli.Localize.Generator.Internal
{
    internal class GeneratorDataContext  
    {
        public string OriginFilePath { get; set; }
        public string Namespace { get; set; }
        public IReadOnlyList<CultureData> CultureData { get; set; }
        public string GeneratedFileName { get; set; }
        public string GeneratedClassName { get; set; }

        public GeneratorDataContext(GeneratorExecutionContext context, AdditionalText originFile)
        {
            this.OriginFilePath = originFile.Path;
            this.Namespace = this.ResolveNamespace(context, originFile);
            this.CultureData = this.ResolveCultureFiles(originFile);
            this.GeneratedClassName = Path.GetFileNameWithoutExtension(originFile.Path);
            this.GeneratedFileName = $"{this.GeneratedClassName}.g.cs";
        }

        private IReadOnlyList<CultureData> ResolveCultureFiles(AdditionalText originFile)
        {
            var searchDir = Path.GetDirectoryName(originFile.Path);
            var fileName = Path.GetFileNameWithoutExtension(originFile.Path);
            return Directory.GetFiles(searchDir, $"{fileName}*{Path.GetExtension(originFile.Path)}")
                .Select(Internal.CultureData.Initialize).ToList();
        }

        private string ResolveNamespace(GeneratorExecutionContext context, AdditionalText originFile)
        {
            if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.rootnamespace", out var rootNamespace))
                rootNamespace = context.Compilation.AssemblyName;

            var namespaceName = rootNamespace;
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.projectdir", out var projectDir))
            {
                namespaceName = Path.GetDirectoryName(originFile.Path)
                   .Replace(projectDir.TrimEnd(Path.DirectorySeparatorChar), rootNamespace)
                   .Replace(Path.DirectorySeparatorChar, '.');
            }

            return namespaceName;
        }
    }
}
