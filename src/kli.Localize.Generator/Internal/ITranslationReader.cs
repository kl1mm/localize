using kli.Localize.Generator.Internal.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace kli.Localize.Generator.Internal
{
    internal interface ITranslationReader
    {
        TranslationData Read(AdditionalText text);
    }
}