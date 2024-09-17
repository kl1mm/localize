using kli.Localize.Generator.Internal.Helper;

namespace kli.Localize.Generator.Internal
{
    internal interface ITranslationReader
    {
        TranslationData Read(string filePath);
    }
}