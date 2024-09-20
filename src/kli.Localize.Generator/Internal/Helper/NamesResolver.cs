using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace kli.Localize.Generator.Internal.Helper
{
    internal class NamesResolver(
        AdditionalText originFile,
        string fallBackRootNamespace,
        AnalyzerConfigOptionsProvider optionsProvider)
    {
        public const string MetaDataNeutralCulture = "NeutralCulture";
        public const string MetaDataClassName = "ClassName";
        public const string MetaDataNamespaceName = "NamespaceName";        
        public const string PropertyRootNamespace = "rootnamespace";
        public const string PropertyProjectDir = "projectdir";

        public string ResolveNeutralCulture()
        {
            optionsProvider.GetOptions(originFile)
                .TryGetValue($"build_metadata.AdditionalFiles.{MetaDataNeutralCulture}", out var neutralCulture);
            return neutralCulture; //TODO: diagnostic error if not given?
        }
        public string ResolveGeneratedClassName()
        {
            if (optionsProvider.GetOptions(originFile).TryGetValue($"build_metadata.AdditionalFiles.{MetaDataClassName}", out var className) && !string.IsNullOrWhiteSpace(className))
                return className;
            return PathHelper.FileNameWithoutCulture(originFile.Path);
        }
        
        public string ResolveGeneratedFileName() 
            => $"{this.ResolveGeneratedClassName()}.g.cs";

        public string ResolveNamespace()
        {
            if (optionsProvider.GetOptions(originFile).TryGetValue($"build_metadata.AdditionalFiles.{MetaDataNamespaceName}", out var namespaceName) && !string.IsNullOrWhiteSpace(namespaceName))
                return namespaceName;

            if (!optionsProvider.GlobalOptions.TryGetValue($"build_property.{PropertyRootNamespace}", out var rootNamespace))
                rootNamespace = fallBackRootNamespace;

            if (optionsProvider.GlobalOptions.TryGetValue($"build_property.{PropertyProjectDir}", out var projectDir))
            {
                var fromPath = this.EnsurePathEndsWithDirectorySeparator(projectDir);
                var toPath = this.EnsurePathEndsWithDirectorySeparator(Path.GetDirectoryName(originFile.Path));
                var relativPath = this.GetRelativePath(fromPath, toPath);

                return $"{rootNamespace}.{relativPath.Replace(Path.DirectorySeparatorChar, '.')}".TrimEnd('.');
            }

            return rootNamespace;
        }

        private string GetRelativePath(string fromPath, string toPath)
        {
            var relativeUri = new Uri(fromPath).MakeRelativeUri(new(toPath));
            return Uri.UnescapeDataString(relativeUri.ToString())
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        private string EnsurePathEndsWithDirectorySeparator(string path) 
            => path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    }
}
