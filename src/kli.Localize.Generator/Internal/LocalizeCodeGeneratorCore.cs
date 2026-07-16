using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kli.Localize.Generator.Internal.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace kli.Localize.Generator.Internal
{
    internal class LocalizeCodeGeneratorCore
    {
        public SourceText CreateClass(GeneratorData generatorData) =>
            SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(generatorData.Namespace))
                .WithLeadingTrivia(Trivia.CreateFileHeader(this.GetType().Assembly.GetName().Name))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Globalization")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")))
                .AddUsings(SyntaxFactory.UsingDirective(
                    SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("Translations")),
                    SyntaxFactory.ParseName("System.Collections.Generic.Dictionary<string, string>")))
                .AddMembers(SyntaxFactory.ClassDeclaration(generatorData.GeneratedClassName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.SealedKeyword))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                    .WithLeadingTrivia(Trivia.CreateClassHeader(generatorData.FileName))
                    .AddMembers(this.LocalizationProviderProperty())
                    .AddMembers(this.GetAllMethod())
                    .AddMembers(this.GetStringMethod())
                    .AddMembers(this.CreateLocalizationProviderClass(generatorData))
                    .AddMembers(this.ProjectTranslationsToMemberDeclarations(generatorData.InvariantTranslationData,
                        generatorData.FileName)))
                .NormalizeWhitespace()
                .GetText(Encoding.UTF8);

        private MemberDeclarationSyntax LocalizationProviderProperty()
            => SyntaxFactory.ParseMemberDeclaration("private static readonly LocalizationProvider provider = new LocalizationProvider();");

        private MemberDeclarationSyntax GetAllMethod()
            => SyntaxFactory.ParseMemberDeclaration("public static IDictionary<string, string> GetAll(CultureInfo cultureInfo = null) => provider.GetValues(cultureInfo ?? CultureInfo.CurrentUICulture);")?
                .WithLeadingTrivia(Trivia.CreateGetAllDocCommentTrivia());

        private MemberDeclarationSyntax GetStringMethod()
            => SyntaxFactory.ParseMemberDeclaration("public static string GetString(string key, CultureInfo cultureInfo = null) => provider.GetValue(key, cultureInfo ?? CultureInfo.CurrentUICulture);")?
                .WithLeadingTrivia(Trivia.CreateGetStringDocCommentTrivia());

        private MemberDeclarationSyntax[] ProjectTranslationsToMemberDeclarations(TranslationData translationData, string fileName, string parentKey = "")
        {
            return (from translation in translationData
                let key = string.IsNullOrEmpty(parentKey) ? translation.Key : StringHelper.Keys.NestedKey(parentKey,translation.Key)
                select translation.Value switch
                {
                    string value => this.CreateTranslationAccessProperty(translation.Key, key, value),
                    TranslationData child => this.CreateNestedTranslationAccessClass(translation.Key, key, fileName, child),
                    _ =>  throw new ArgumentOutOfRangeException(nameof(translation.Value))
                }).ToArray();
        }



        private MemberDeclarationSyntax CreateNestedTranslationAccessClass(string className, string translationKey, string fileName, TranslationData next)
        {
          return SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .WithLeadingTrivia(Trivia.CreateNestedClassHeader(fileName, translationKey))
                .AddMembers(this.CreateNestedGetStringMethod(translationKey))
                .AddMembers(this.ProjectTranslationsToMemberDeclarations(next, fileName, translationKey));
        }

        private MemberDeclarationSyntax CreateNestedGetStringMethod(string translationSection)
        {
            var keyPrefix = StringHelper.Keys.NestedKey(translationSection, string.Empty);
            var member = $$"""
                           public static string GetString(string key, CultureInfo cultureInfo = null)
                               => provider.GetValue("{{keyPrefix}}" + key, cultureInfo ?? CultureInfo.CurrentUICulture);
                           """;
            return SyntaxFactory.ParseMemberDeclaration(member)
                .WithLeadingTrivia(Trivia.CreateGetStringDocCommentTrivia());
        }
        
        private MemberDeclarationSyntax CreateTranslationAccessProperty(string propertyName, string translationKey, string translationValue)
        {
            var member = $"""
                public static string {propertyName} 
                    => provider.GetValue("{translationKey}", CultureInfo.CurrentUICulture); 
                """;

            return SyntaxFactory.ParseMemberDeclaration(member)!
                .WithLeadingTrivia(Trivia.CreateMemberHeader(translationValue));
        }
        
        private ClassDeclarationSyntax CreateLocalizationProviderClass(GeneratorData context)
        {
            var syntaxNode = CSharpSyntaxTree.ParseText(
                @"
                    private class LocalizationProvider 
                    {
                        delegate bool SelectorFunc<T>(Translations translations, out T arg);

                        internal string GetValue(string key, CultureInfo cultureInfo)
                        {
                            bool ValueSelector(Translations translations, out string value)
                            {
                                if (translations.TryGetValue(key, out value))
                                    return true;

                                value = key;
                                return false;
                            }

                            return TraverseCultures<string>(cultureInfo, ValueSelector);
                        }

                        internal IDictionary<string, string> GetValues(CultureInfo cultureInfo)
                        {
                            bool ValueSelector(Translations translations, out Translations value)
                            {
                                value = translations;
                                return true;
                            }

                            return TraverseCultures<Translations>(cultureInfo, ValueSelector);
                        }

                        private T TraverseCultures<T>(CultureInfo cultureInfo, SelectorFunc<T> selectorFunc)
                        {
                            if (resources.TryGetValue(cultureInfo, out Translations translations))
                            {
                                if (selectorFunc(translations, out T result) || cultureInfo == CultureInfo.InvariantCulture)
                                    return result;
                            }

                            return TraverseCultures<T>(cultureInfo.Parent, selectorFunc);
                        }
                    }   
                ").GetRoot();

            return ((ClassDeclarationSyntax)syntaxNode.DescendantNodes().First())
                .AddMembers(GetTranslationDictionaryFieldDeclaration(context))
                .AddMembers(GetResourceDictionaryFieldDeclaration(context.CultureData));
        }

        private MemberDeclarationSyntax[] GetTranslationDictionaryFieldDeclaration(GeneratorData context)
        {
            return context.CultureData
                .Select(cd =>
                {
                    var entries = cd.Translations
                        .Flatten()
                        .Select(t => $"{{ \"{t.Key}\", \"{StringHelper.EscapeValue(t.Value)}\" }},");
                    
                    return $@"private static readonly Translations {cd.NormalizedKey} = new()
                            {{
                                {string.Join("\r\n", entries)}
                            }};";
                })
                .Select(source => SyntaxFactory.ParseMemberDeclaration(source))
                .ToArray();
        }

        private MemberDeclarationSyntax GetResourceDictionaryFieldDeclaration(IReadOnlyList<CultureData> cultureData)
        {
            var entries = cultureData.Select(cd =>
            {
                var key = cd.Key == CultureData.InvariantKeyName ? "CultureInfo.InvariantCulture" : $"new CultureInfo(\"{cd.Key}\")";
                return $"{{ {key}, {cd.NormalizedKey} }},";
            });
            var source = $@"private static readonly Dictionary<CultureInfo, Translations> resources = new()
                            {{
                                {string.Join("\r\n", entries)}
                            }};";

            return SyntaxFactory.ParseMemberDeclaration(source);
        }
    }
}