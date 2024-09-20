using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace kli.Localize.Generator.Internal.Helper
{
    internal class CultureData
    {
        public const string InvariantKeyName = "invariant";
        
        public string Key { get; private set; }
        public string NormalizedKey { get; private set; }
        public TranslationData Translations { get; private set; }

        private CultureData() { }

        public static IReadOnlyList<CultureData> Initialize(string neutralCulture, IReadOnlyList<AdditionalText> additionalTexts, ITranslationReader translationReader)
        {
            return additionalTexts.Select(at => ResolveCulture(neutralCulture, at, translationReader)).ToList();
        }
        
        private static CultureData ResolveCulture(string neutralCulture, AdditionalText additionalText, ITranslationReader translationReader)
        {
            var cultureId = Path.GetFileNameWithoutExtension(additionalText.Path).Split('_').Skip(1).LastOrDefault();

            if (cultureId == neutralCulture)
                cultureId = InvariantKeyName;
            
            return new()
            {
                Key = cultureId, 
                NormalizedKey = NormalizeCultureIdentifier(cultureId), 
                Translations = translationReader.Read(additionalText)
            };
        }

        private static string NormalizeCultureIdentifier(string cultureId)
            => cultureId.ToLower().Replace('-', '_');
    }
}
