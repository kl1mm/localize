using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace kli.Localize.Generator.Internal.Json
{
    internal static class JsonDiagnostics
    {
        public static void ReportInvalidFileFormat(this Action<Diagnostic> reporter, string filePath, JsonReaderException ex)
        {
            reporter.Report(1, ex.Message, filePath, 
                new LinePosition(ex.LineNumber-1, ex.LinePosition), DiagnosticSeverity.Error);
        }
        
        public static void ReportInvalidKey(this Action<Diagnostic> reporter, string filePath, JsonTextReader reader)
        {
            var message = $"Json property key must be a valid C# identifier: '{reader.Value}'";
            reporter.Report(2, message, filePath, 
                new LinePosition(reader.LineNumber-1, reader.LinePosition), DiagnosticSeverity.Warning);
        } 
        
        public static void ReportInvalidTokenType(this Action<Diagnostic> reporter, string filePath, JsonTextReader reader)
        {
            var message = $"Json property value must be an object or a string: '{reader.TokenType} - {reader.Value} ({reader.ValueType})'";
            reporter.Report(3, message, filePath, 
                new LinePosition(reader.LineNumber-1, reader.LinePosition), DiagnosticSeverity.Warning);
        }

        private static void Report(this Action<Diagnostic> reporter, int id, string message,
            string filePath, LinePosition linePosition, DiagnosticSeverity severity)
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor($"SGL000{id}", "kli.Localize.Generator", message, "Source Generators", severity, true),
                Location.Create(filePath, new TextSpan(), new LinePositionSpan(linePosition, linePosition)));
            reporter(diagnostic);
        }
    }
}