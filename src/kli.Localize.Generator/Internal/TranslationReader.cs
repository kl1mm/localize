using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using YamlDotNet.Core;
using Translations = System.Collections.Generic.Dictionary<string, string>;

namespace kli.Localize.Generator.Internal
{
    internal class TranslationReader
    {
        private readonly Action<Diagnostic> reportDiagnostic;

        public TranslationReader(Action<Diagnostic> reportDiagnostic)
        {
            this.reportDiagnostic = reportDiagnostic;
        }

        public Translations Read(string filePath)
        {
            try
            {
                var allTranslations = ReadTranslations(filePath);
                var validTranslations = allTranslations
                    .Where(t => this.IsValidIdentifier(t.Key));

                foreach (var invalid in allTranslations.Except(validTranslations))
                {
                    var match = File.ReadLines(filePath)
                        .Select((line, index) => new {line, lineNumber = index})
                        .FirstOrDefault(x => x.line.Contains(invalid.Key) && x.line.Contains(invalid.Value));

                    this.ReportDiagnostic(filePath,
                        $"Json property key must be a valid C# identifier: '{invalid.Key}'",
                        match?.lineNumber ?? 0);
                }

                return validTranslations.ToDictionary(t => t.Key, t => t.Value);
            }
            catch (JsonReaderException ex)
            {
                throw new FormatException($"Failed to parse json: '{filePath}'. Only string values are allowed. InnerException: '{ex.Message}'", ex);
            }
        }

        private static Translations? ReadTranslations(string filePath)
        {
            var lower = Path.GetExtension(filePath).ToLower();
            return lower switch
            {
                ".json" => FromJson(filePath),
                ".yml" => FromYaml(filePath),
                ".yaml" => FromYaml(filePath),
                ".xml" => FromResx(filePath),
                ".resx" => FromResx(filePath),
                _ => FromJson(filePath)
            };
        }

        private static Translations? FromJson(string filePath)
        {
            var allTranslations = JsonConvert.DeserializeObject<Translations>(File.ReadAllText(filePath));
            return allTranslations;
        }

        private static Translations? FromResx(string filename)
        {
            var result = new Translations();
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            foreach (XmlNode node in doc.SelectNodes("//root/data"))
            {
                var value = node.SelectSingleNode("value").InnerText.Replace('"', '\'');
                var key = node.Attributes["name"].Value;
                result.Add(key, value);
            }

            return result;
        }

        private static Translations? FromYaml(string filename)
        {
            var serializer = new YamlDotNet.Serialization.Deserializer();
            return serializer.Deserialize<Translations>(File.ReadAllText(filename));
        }
        
        private bool IsValidIdentifier(string identifier)
        {
            identifier = PropertyNameChangePattern.Change(identifier);
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
    }
}
