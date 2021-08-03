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

            var namespaceName = rootNamespace;
            if (this.optionsTryGetFunc("build_property.projectdir", out var projectDir))
            {
                namespaceName = Path.GetDirectoryName(originFilePath)
                   .Replace(projectDir.TrimEnd(Path.DirectorySeparatorChar), rootNamespace)
                   .Replace(Path.DirectorySeparatorChar, '.');
            }

            return namespaceName;
        }
    }
}
