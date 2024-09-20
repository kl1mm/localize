// using System.Diagnostics;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using System.Threading;
// using kli.Localize.Generator;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.Diagnostics;
// using Microsoft.CodeAnalysis.Text;
// using NSubstitute;
// using Shouldly;
// using Xunit;
//
// namespace kli.Localize.Test;
//
// public class My : AdditionalText
// {
//     private const string FileName = "TestLocale_de.json";
//     private static readonly SourceText SourceText = SourceText.From("""
//                                                                     {
//                                                                         "Errors": 
//                                                                         {
//                                                                             "FileNotFound": "Datei nicht gefunden",
//                                                                             "DivideByZero": "Durch Null geteilt"
//                                                                         },
//                                                                         "Warnings":{
//                                                                             "Caution": "Achtung"
//                                                                         },
//                                                                         "UI":{
//                                                                             "LabelOne": "Eins",
//                                                                             "LabelTwo": "Zwei",
//                                                                             "Login": {
//                                                                                 "LabelUserName": "Benutzer",
//                                                                                 "LabelPassword": "Passwort"
//                                                                             }
//                                                                         }
//                                                                     }
//                                                                     """);  
//     public override SourceText GetText(CancellationToken cancellationToken = default) 
//         => SourceText;
//
//     public override string Path => FileName!;
// }
//
// public class GeneratorTest
// {
//     [Fact]
//     public void SimpleGeneratorTest()
//     {
//         // Create the 'input' compilation that the generator will act on
//         var inputCompilation = CreateCompilation(@"
// namespace MyCode
// {
//     public class Program
//     {
//         public static void Main(string[] args)
//         {
//         }
//     }
// }
// ");
//         var additionalText = new My();
//         var otherAdditionaltextMock = Substitute.For<AdditionalText>();
//         otherAdditionaltextMock.Path.Returns("doesNotMatterExceptFor.json");
//         var generator = new LocalizeCodeGenerator().AsSourceGenerator();
//         var configOptions = CreateAnalyzerConfigOptionsProviderMock(additionalText.Path,"C:\\project\\dir\\", "My.Project", "de");
//         GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], [additionalText, otherAdditionaltextMock]).WithUpdatedAnalyzerConfigOptions(configOptions);
//         driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);
//
//         diagnostics.IsEmpty.ShouldBeTrue(); // there were no diagnostics created by the generators
//         outputCompilation.GetDiagnostics().IsEmpty.ShouldBeTrue();
//
//         var runResult = driver.GetRunResult();  // Or we can look at the results directly:
//         runResult.Diagnostics.IsEmpty.ShouldBeTrue();
//
//         var generatorResult = runResult.Results[0];
//         Debug.Assert(generatorResult.Generator == generator);
//         Debug.Assert(generatorResult.Diagnostics.IsEmpty);
//         Debug.Assert(generatorResult.GeneratedSources.Length == 1);
//         Debug.Assert(generatorResult.Exception is null);
//     }
//
//     private static Compilation CreateCompilation(string source)
//         => CSharpCompilation.Create("compilation",
//             new[] { CSharpSyntaxTree.ParseText(source) },
//             new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
//             new CSharpCompilationOptions(OutputKind.ConsoleApplication));
//     
//     internal AnalyzerConfigOptionsProvider CreateAnalyzerConfigOptionsProviderMock(string additionalTextPath, string projectDir, string rootNameSpace, string? neutralCulture = null)
//     {
//         var configOptionsMock = Substitute.For<AnalyzerConfigOptions>();
//         configOptionsMock.TryGetValue("build_property.projectdir", out Arg.Any<string>()!)
//             .Returns(x => { x[1] = projectDir; return true; });
//         configOptionsMock.TryGetValue("build_property.rootnamespace", out Arg.Any<string>()!)
//             .Returns(x => { x[1] = rootNameSpace; return true; });
//         configOptionsMock.TryGetValue("build_metadata.AdditionalFiles.NeutralCulture", out Arg.Any<string>()!)
//             .Returns(x => { x[1] = neutralCulture; return true; });
//
//         var providerMock = Substitute.For<AnalyzerConfigOptionsProvider>();
//         providerMock.GetOptions(Arg.Is<AdditionalText>(at => at.Path == additionalTextPath))
//             .Returns(configOptionsMock);
//         providerMock.GlobalOptions.Returns(configOptionsMock);
//
//         return providerMock;
//     }
// }