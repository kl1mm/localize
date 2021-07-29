using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using Translations = System.Collections.Generic.Dictionary<string, string>;

namespace kli.Localize.Generator.Internal
{
    internal class LocalizeCodeGeneratorCore
    {
        private readonly Action<Diagnostic> reportDiagnostic;

        public LocalizeCodeGeneratorCore(Action<Diagnostic> reportDiagnostic)
        {
            this.reportDiagnostic = reportDiagnostic;
        }

        public SourceText CreateClass(GeneratorDataContext context)
        {
            var translations = this.ReadTranslations(context.OriginFilePath);

            var classDeclaration = SyntaxFactory.ClassDeclaration(context.GeneratedClassName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.SealedKeyword))
                .AddMembers(this.LocalizationProviderProperty())
                .AddMembers(this.GetAllMethod())
                .AddMembers(this.GetStringMethod())
                .AddMembers(translations.Select(this.ProjectTranslationToMemberDeclaration).ToArray())
                .AddMembers(this.CreateLocalizationProviderClass(context.CultureData));

            var sourceAsString = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(context.Namespace))
                .WithLeadingTrivia(SyntaxFactory.Comment(this.CreateFileHeader()))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Globalization")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("Translations")), SyntaxFactory.ParseName("System.Collections.Generic.Dictionary<string, string>")))
                .AddMembers(classDeclaration)
                .NormalizeWhitespace()
                .ToFullString();

            return SourceText.From(sourceAsString, Encoding.UTF8);
        }

        private MemberDeclarationSyntax LocalizationProviderProperty()
            => SyntaxFactory.ParseMemberDeclaration("private static readonly LocalizationProvider provider = new LocalizationProvider();");

        private MemberDeclarationSyntax GetAllMethod()
            => SyntaxFactory.ParseMemberDeclaration("public static IDictionary<string, string> GetAll(CultureInfo cultureInfo = null) => provider.GetValues(cultureInfo ?? CultureInfo.CurrentUICulture);");

        private MemberDeclarationSyntax GetStringMethod()
            => SyntaxFactory.ParseMemberDeclaration("public static string GetString(string key, CultureInfo cultureInfo = null) => provider.GetValue(key, cultureInfo ?? CultureInfo.CurrentUICulture);");

        private MemberDeclarationSyntax ProjectTranslationToMemberDeclaration(KeyValuePair<string, string> translation)
            => SyntaxFactory.ParseMemberDeclaration($"public static string {translation.Key} => provider.GetValue(nameof({translation.Key}), CultureInfo.CurrentUICulture);")
            .WithLeadingTrivia(SyntaxFactory.Comment(this.CreateMemberHeader(translation.Value)));

        private ClassDeclarationSyntax CreateLocalizationProviderClass(IReadOnlyList<CultureData> cultureData)
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
                            while (cultureInfo != CultureInfo.InvariantCulture)
                            {
                                if (resources.TryGetValue(cultureInfo, out Translations translations))
                                {
                                    if (selectorFunc(translations, out T result))
                                        return result;
                                }
                                cultureInfo = cultureInfo.Parent;
                            }

                            selectorFunc(resources[CultureInfo.InvariantCulture], out T retVal);
                            return retVal;
                        }
                    }   
                ").GetRoot();

            return ((ClassDeclarationSyntax)syntaxNode.DescendantNodes().First())
                .AddMembers(cultureData.Select(GetTranslationDictionaryFieldDeclaration).ToArray())
                .AddMembers(GetResourceDictionaryFieldDeclaration(cultureData));
        }

        private MemberDeclarationSyntax GetTranslationDictionaryFieldDeclaration(CultureData ctx)
        {
            var translations = this.ReadTranslations(ctx.FilePath);
            var entries = translations.Select(t => $"{{ \"{t.Key}\", {t.Value} }},");
            var source = $@"private static readonly Translations {ctx.Normalized} = new()
                            {{
                                {string.Join("\r\n", entries)}
                            }};";

            return SyntaxFactory.ParseMemberDeclaration(source);
        }

        private MemberDeclarationSyntax GetResourceDictionaryFieldDeclaration(IEnumerable<CultureData> cultureData)
        {
            var entries = cultureData.Select(cd => $"{{ {cd.Key}, {cd.Normalized} }},");
            var source = $@"private static readonly Dictionary<CultureInfo, Translations> resources = new()
                            {{
                                {string.Join("\r\n", entries)}
                            }};";

            return SyntaxFactory.ParseMemberDeclaration(source);
        }

        private Translations ReadTranslations(string filePath)
        {
            try
            {
                var allTranslations = JsonConvert.DeserializeObject<Translations>(File.ReadAllText(filePath));
                var validTranslations = allTranslations
                               .Where(t => this.IsValidIdentifier(t.Key));

                foreach (var invalid in allTranslations.Except(validTranslations))
                {
                    var match = File.ReadLines(filePath)
                        .Select((line, index) => new { line, lineNumber = index })
                        .FirstOrDefault(x => x.line.Contains(invalid.Key) && x.line.Contains(invalid.Value));

                    this.ReportDiagnostic(filePath,
                        $"Json property key must be a valid C# identifier: '{invalid.Key}'",
                        match?.lineNumber ?? 0);
                }

                return validTranslations.ToDictionary(t => t.Key, t => JsonConvert.ToString(t.Value));
            }
            catch (JsonReaderException ex)
            {
                throw new JsonException($"Failed to parse json: '{filePath}'. Only string values are allowed. InnerException: '{ex.Message}'", ex);
            }
        }

        private bool IsValidIdentifier(string identifier)
        {
            return SyntaxFacts.IsValidIdentifier(identifier)
                && SyntaxFacts.GetKeywordKind(identifier) == SyntaxKind.None
                && SyntaxFacts.GetContextualKeywordKind(identifier) == SyntaxKind.None;
        }

        private void ReportDiagnostic(string filePath, string message, int line = 0,
            DiagnosticSeverity severity = DiagnosticSeverity.Warning)
        {
            var linePosition = new LinePosition(line, 0);
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor("SGL0001", "Source Generators", message, "Source Generators", severity, true),
                Location.Create(filePath, new TextSpan(), new LinePositionSpan(linePosition, linePosition)));

            this.reportDiagnostic(diagnostic);
        }

        private string CreateMemberHeader(string value) => $"///<summary>Similar to: {value}</summary>";

        private string CreateFileHeader()
        {
            return string.Format(@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by {0}.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------"
            , this.GetType().Assembly.GetName().Name);
        }
    }
}
