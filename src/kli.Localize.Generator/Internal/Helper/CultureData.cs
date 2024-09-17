using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace kli.Localize.Generator.Internal.Helper
{
    internal class CultureData
    {
        public const string InvariantKeyName = "invariant";
        
        public string Key { get; private set; }
        public string NormalizedKey { get; private set; }
        public TranslationData Translations { get; private set; }

        private CultureData() { }

        public static IReadOnlyList<CultureData> Initialize(string filePath, ITranslationReader translationReader)
        {
            var searchDir = Path.GetDirectoryName(filePath)!;
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            return Directory.GetFiles(searchDir, $"{fileName}*{Path.GetExtension(filePath)}")
                .Select(cfp => ResolveCulture(cfp, translationReader)).ToList();
        }
        
        private static CultureData ResolveCulture(string cultureFilePath, ITranslationReader translationReader)
        {
            var cultureId = Path.GetFileNameWithoutExtension(cultureFilePath).Split('_').Skip(1).LastOrDefault()
                ?? InvariantKeyName;
            
            return new()
            {
                Key = cultureId, 
                NormalizedKey = NormalizeCultureIdentifier(cultureId), 
                Translations = translationReader.Read(cultureFilePath)
            };
        }

        private static string NormalizeCultureIdentifier(string cultureId)
            => cultureId.ToLower().Replace('-', '_');
    }
}
