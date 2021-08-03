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

        public GeneratorDataContext(AdditionalText originFile, NamespaceResolver namespaceResolver)
        {
            this.OriginFilePath = originFile.Path;
            this.CultureData = this.ResolveCultureFiles(originFile);
            this.GeneratedClassName = Path.GetFileNameWithoutExtension(originFile.Path);
            this.GeneratedFileName = $"{this.GeneratedClassName}.g.cs";
            this.Namespace = namespaceResolver.Resolve();
        }

        private IReadOnlyList<CultureData> ResolveCultureFiles(AdditionalText originFile)
        {
            var searchDir = Path.GetDirectoryName(originFile.Path);
            var fileName = Path.GetFileNameWithoutExtension(originFile.Path);
            return Directory.GetFiles(searchDir, $"{fileName}*{Path.GetExtension(originFile.Path)}")
                .Select(Internal.CultureData.Initialize).ToList();
        }
    }
}
