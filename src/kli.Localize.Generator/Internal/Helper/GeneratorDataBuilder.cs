using Microsoft.CodeAnalysis;

namespace kli.Localize.Generator.Internal.Helper;

internal class GeneratorDataBuilder(AdditionalText originFile, 
    NamesResolver namesResolver, ITranslationReader translationReader)
{
    public GeneratorData Build()
    {
        return new GeneratorData
        {
            GeneratedClassName = namesResolver.ResolveGeneratedClassName(),
            GeneratedFileName = namesResolver.ResolveGeneratedFileName(),
            Namespace = namesResolver.ResolveNamespace(),
            CultureData = CultureData.Initialize(originFile.Path, translationReader),
        };
    }
}