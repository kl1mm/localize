using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using kli.Localize.Generator.Base;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace kli.Localize.Generator.Internal
{
    internal class LocalizeCodeGeneratorCore
    {
        private readonly TranslationReader translationReader;

        public LocalizeCodeGeneratorCore(TranslationReader translationReader)
        {
            this.translationReader = translationReader;
        }

        public SourceText CreateClass(GeneratorDataContext context)
        {
            var translations = this.translationReader.Read(context.OriginFilePath);

            var classDeclaration = SyntaxFactory.ClassDeclaration(context.GeneratedClassName)
                    .AddBaseListTypes(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"GeneratedLocalizationBase<{context.GeneratedClassName}>")),
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(nameof(IGeneratedLocalization))))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.SealedKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(this.LocalizationProviderField())
                .AddMembers(this.LocalizationProviderProperty())
                    .AddMembers(translations.Select(this.ProjectTranslationToMemberDeclaration).ToArray())
                .AddMembers(this.CreateLocalizationProviderClass(context.CultureData));

            var sourceAsString = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(context.Namespace))
                .WithLeadingTrivia(SyntaxFactory.Comment(this.CreateFileHeader()))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IGeneratedLocalization).Namespace)))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Globalization")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("Translations")), SyntaxFactory.ParseName("System.Collections.Generic.Dictionary<string, string>")))
                .AddMembers(classDeclaration)
                .NormalizeWhitespace()
                .ToFullString();

            return SourceText.From(sourceAsString, Encoding.UTF8);
        }

        private MemberDeclarationSyntax LocalizationProviderField()
            => SyntaxFactory.ParseMemberDeclaration($"private readonly LocalizationProvider provider = new LocalizationProvider();");

        private MemberDeclarationSyntax LocalizationProviderProperty()
            => SyntaxFactory.ParseMemberDeclaration($"protected override LocalizationProviderBase Provider => provider;");

        private MemberDeclarationSyntax ProjectTranslationToMemberDeclaration(KeyValuePair<string, string> translation)
            => SyntaxFactory.ParseMemberDeclaration($"public static string {PropertyNameChangePattern.Change(translation.Key)} => GetString(\"{translation.Key}\", CultureInfo.CurrentUICulture);")
            .WithLeadingTrivia(SyntaxFactory.Comment(this.CreateMemberHeader(translation.Value)));

        private ClassDeclarationSyntax CreateLocalizationProviderClass(IReadOnlyList<CultureData> cultureData)
        {
            var syntaxNode = CSharpSyntaxTree.ParseText(
                @"
                    private class LocalizationProvider : GeneratedLocalizationBase.LocalizationProviderBase
                    {
                        
                    }   
                ").GetRoot();

            return ((ClassDeclarationSyntax)syntaxNode.DescendantNodes().First())
                .AddMembers(cultureData.Select(GetTranslationDictionaryFieldDeclaration).ToArray())
                .AddMembers(GetResourceDictionaryFieldDeclaration(cultureData));
        }

        private MemberDeclarationSyntax GetTranslationDictionaryFieldDeclaration(CultureData ctx)
        {
            var translations = this.translationReader.Read(ctx.FilePath);
            var entries = translations.Select(t => $"{{ \"{t.Key}\", {JsonConvert.ToString(t.Value)} }},");
            var source = $@"private static readonly Translations {ctx.Normalized} = new()
                            {{
                                {string.Join("\r\n", entries)}
                            }};";

            return SyntaxFactory.ParseMemberDeclaration(source);
        }

        private MemberDeclarationSyntax GetResourceDictionaryFieldDeclaration(IEnumerable<CultureData> cultureData)
        {
            var entries = cultureData.Select(cd => $"{{ {cd.Key}, {cd.Normalized} }},");
            var source = $@"protected override Dictionary<CultureInfo, Translations> resources {{ get; }} = new()
                            {{
                                {string.Join("\r\n", entries)}
                            }};";

            return SyntaxFactory.ParseMemberDeclaration(source);
        }

        private string CreateMemberHeader(string value) => $"///<summary>Similar to: {value}</summary>";

        private string CreateFileHeader()
        {
            return $@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by {this.GetType().Assembly.GetName().Name}. at {DateTime.Now} with Version {typeof(LocalizeCodeGeneratorCore).Assembly.GetName().Version}
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------";
        }
    }
}
