using System;
using System.Collections.Generic;
using System.IO;
using kli.Localize.Generator.Internal.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;

namespace kli.Localize.Generator.Internal.Json
{
    internal class JsonTranslationReader(Action<Diagnostic> reportDiagnostic) : ITranslationReader
    {
        public TranslationData Read(string filePath)
        {
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open);
                var reader = new JsonTextReader(new StreamReader(fileStream));
                return this.ReadCore(filePath, reader);
            }
            catch (JsonReaderException ex)
            {
                reportDiagnostic.ReportInvalidFileFormat(filePath, ex);
            }

            return new TranslationData();
        }

        private TranslationData ReadCore(string filePath, JsonTextReader reader)
        {
            var hierachie = new Stack<TranslationData>([new TranslationData()]);
            var propertyName = string.Empty;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                        HandleStartObject(reader, hierachie, propertyName);
                        break;
                    case JsonToken.PropertyName:
                        this.HandleProperty(filePath, reader, ref propertyName);
                        break;
                    case JsonToken.String:
                        hierachie.Peek().Add(propertyName, reader.Value!.ToString());
                        break;
                    case JsonToken.EndObject:
                        HandleEndObject(reader, hierachie);
                        break;
                    case JsonToken.None:
                    case JsonToken.Comment:
                        reader.Skip();
                        break;
                    default:
                        reportDiagnostic.ReportInvalidTokenType(filePath, reader);
                        reader.Skip();
                        break;
                }
            }

            return hierachie.Pop();
        }

        private static void HandleEndObject(JsonTextReader reader, Stack<TranslationData> hierachie)
        {
            if (reader.Depth > 0)
                hierachie.Pop();
        }

        private void HandleProperty(string filePath, JsonTextReader reader, ref string propertyName)
        {
            var identifier = reader.Value!.ToString();
            if (!this.IsValidIdentifier(identifier))
            {
                reportDiagnostic.ReportInvalidKey(filePath, reader);
                reader.Skip();
            }
            else
                propertyName = identifier;
        }

        private static void HandleStartObject(JsonTextReader reader, Stack<TranslationData> hierachie, string propertyName)
        {
            if (reader.Depth > 0)
            {
                var currentDepth = new TranslationData();
                hierachie.Peek().Add(propertyName, currentDepth);
                hierachie.Push(currentDepth);
            }
        }

        private bool IsValidIdentifier(string identifier) =>
            SyntaxFacts.IsValidIdentifier(identifier)
            && SyntaxFacts.GetKeywordKind(identifier) == SyntaxKind.None
            && SyntaxFacts.GetContextualKeywordKind(identifier) == SyntaxKind.None;
    }
}