
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
            var cultureId = Path.GetFileNameWithoutExtension(cultureFileName).Split('_').Skip(1).LastOrDefault();
            if (string.IsNullOrEmpty(cultureId))
                return new() { Key = "CultureInfo.InvariantCulture",  Normalized = "invariant", FilePath = cultureFileName };

            return new() { 
                Key = $"new CultureInfo(\"{cultureId}\")", 
                Normalized = NormalizeCultureIdentifier(cultureId), 
                FilePath = cultureFileName 
            };
        }

        private static string NormalizeCultureIdentifier(string cultureId)
            => cultureId.ToLower().Replace('-', '_');
    }
}
