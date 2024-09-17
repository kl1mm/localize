using System.Collections.Generic;
using System.Linq;

namespace kli.Localize.Generator.Internal.Helper
{
    internal class GeneratorData  
    {
        public IReadOnlyList<CultureData> CultureData { get; set; }
        public TranslationData InvariantTranslationData 
            => this.CultureData.Single(cd => cd.Key == Helper.CultureData.InvariantKeyName).Translations;
        
        public string Namespace { get; set; }
        public string GeneratedFileName { get; set; }
        public string GeneratedClassName { get; set; }
    }
}
