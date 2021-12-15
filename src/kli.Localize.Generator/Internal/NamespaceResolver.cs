using System;
using System.IO;

namespace kli.Localize.Generator.Internal
{
    internal class NamespaceResolver
    {
        public delegate bool OptionsTryGetFunc(string key, out string value);

        private string originFilePath;
        private readonly string fallBackRootNamespace;
        private readonly OptionsTryGetFunc optionsTryGetFunc;

        public NamespaceResolver(string originFilePath, string fallBackRootNamespace, OptionsTryGetFunc optionsGetterFunc)
        {
            this.originFilePath = originFilePath;
            this.fallBackRootNamespace = fallBackRootNamespace;
            this.optionsTryGetFunc = optionsGetterFunc;
        }

        public string Resolve()
        {
            if (!this.optionsTryGetFunc("build_property.rootnamespace", out var rootNamespace))
                rootNamespace = fallBackRootNamespace;

            if (this.optionsTryGetFunc("build_property.projectdir", out var projectDir))
            {
                var fromPath = this.EnsurePathEndsWithDirectorySeparator(projectDir);
                var toPath = this.EnsurePathEndsWithDirectorySeparator(Path.GetDirectoryName(this.originFilePath));
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
