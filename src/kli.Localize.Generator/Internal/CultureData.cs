
using System.IO;
using System.Linq;

namespace kli.Localize.Generator.Internal
{
    internal class CultureData
    {
        public string Key { get; private set; }
        public string Normalized { get; private set; }
        public string FilePath { get; private set; }

        private CultureData() { }

        public static CultureData Initialize(string cultureFileName)
        {
            var cultureId = ReadCultureNameFromFileName(cultureFileName);
            if (string.IsNullOrEmpty(cultureId))
                return new() { Key = "CultureInfo.InvariantCulture",  Normalized = "invariant", FilePath = cultureFileName };

            return new() { 
                Key = $"new CultureInfo(\"{cultureId}\")", 
                Normalized = NormalizeCultureIdentifier(cultureId), 
                FilePath = cultureFileName 
            };
        }

        internal static string ReadCultureNameFromFileName(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName).Split('_', '.').Skip(1).LastOrDefault();
        }

        private static string NormalizeCultureIdentifier(string cultureId)
            => cultureId.ToLower().Replace('-', '_');
    }
}
