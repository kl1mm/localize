using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Translations = System.Collections.Generic.IDictionary<string, string>;

namespace kli.Localize.Tool.Internal
{
    public class CodeGenerator
    {
        public string CreateClass(string rootNamespaceName, string jsonFilePath)
        {
            var translations = JsonSerializer.Deserialize<Translations>(File.ReadAllText(jsonFilePath));
            var className = Path.GetFileNameWithoutExtension(jsonFilePath);
            var namespaceName = string.Join('.', Path.GetDirectoryName(jsonFilePath).Split(Path.PathSeparator).Prepend(rootNamespaceName)).TrimEnd('.');

            var classDeclaration = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(this.LocalizationProviderProperty())
                .AddMembers(translations.Select(this.ProjectTranslationToMemberDeclaration).ToArray())
                .AddMembers(this.CreateLocalizationProviderClass(className));

            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceName))
                .WithLeadingTrivia(SyntaxFactory.Comment(this.CreateFileHeader()))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.IO")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Globalization")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("Translations")), SyntaxFactory.ParseName("System.Collections.Generic.IDictionary<string, string>")))
                .AddMembers(classDeclaration)
                .NormalizeWhitespace()
                .ToFullString();
        }

        private MemberDeclarationSyntax LocalizationProviderProperty()
            => SyntaxFactory.ParseMemberDeclaration("private static readonly LocalizationProvider provider = new LocalizationProvider();");

        private MemberDeclarationSyntax ProjectTranslationToMemberDeclaration(KeyValuePair<string, string> translation)
            => SyntaxFactory.ParseMemberDeclaration($"public static string {translation.Key} => provider.GetValue(nameof({translation.Key}), CultureInfo.CurrentUICulture);");

        private ClassDeclarationSyntax CreateLocalizationProviderClass(string className)
        {
            var syntaxNode = CSharpSyntaxTree.ParseText(
                @"
                    private class LocalizationProvider 
                    {
                        private static readonly IDictionary<CultureInfo, Translations> resources = new Dictionary<CultureInfo, Translations>();
                    
                        public string GetValue(string key, CultureInfo cultureInfo)
                        {
                            if (this.GetTranslations(cultureInfo).TryGetValue(key, out var value))
                                return value;
                            return key;
                        }

                        private Translations GetTranslations(CultureInfo cultureInfo)
                        {
                            if (!resources.TryGetValue(cultureInfo, out var translations))
                            {
                                translations = Load(cultureInfo);
                                resources.Add(cultureInfo, translations);
                            }
                            return translations;
                        }
       
                        private Translations Load(CultureInfo cultureInfo)
                        {
                            return LoadResources(cultureInfo)
                                .SelectMany(dict => dict)
                                .ToLookup(pair => pair.Key, pair => pair.Value)
                                .ToDictionary(group => group.Key, group => group.First());
                        }

                        private IEnumerable<Translations> LoadResources(CultureInfo cultureInfo)
                        {
                            while (cultureInfo != CultureInfo.InvariantCulture)
                            {
                                yield return LoadResource(cultureInfo);
                                cultureInfo = cultureInfo.Parent;
                            }
                            yield return LoadResource(CultureInfo.InvariantCulture);
                        }

                        private Translations LoadResource(CultureInfo cultureInfo)
                        {
                            var resourceName = $""{typeof(" + className + @").FullName}.json"";
                            if (cultureInfo != CultureInfo.InvariantCulture)
                                resourceName = resourceName.Replace("".json"", $""_{cultureInfo.Name}.json"");

                            var assembly = typeof(" + className + @").Assembly;
                            if (assembly.GetManifestResourceNames().Any(n => n.Equals(resourceName)))
                            {
                                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                                using (StreamReader reader = new StreamReader(stream ?? Stream.Null))
                                    return JsonSerializer.Deserialize<Translations>(reader.ReadToEnd());
                            }
                            return new Dictionary<string, string>();
                        }
                    }   
                ").GetRoot();

            return (ClassDeclarationSyntax)syntaxNode.DescendantNodes().First();
        }

        private string CreateFileHeader()
        {
            return string.Format(@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by {0} on {1}.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------"
            , this.GetType().Assembly.GetName().Name, DateTime.Now.ToString());
        }
    }
}