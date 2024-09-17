// using System.Diagnostics;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using System.Threading;
// using kli.Localize.Generator;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.Text;
// using Shouldly;
// using Xunit;
//
// namespace kli.Localize.Test;
//
// public class My : AdditionalText
// {
//     private readonly string _filePath;
//     public My()
//     {
//         var localTestData = """
//             {
//                 "Errors": 
//                 {
//                     "FileNotFound": "Datei nicht gefunden",
//                     "DivideByZero": "Durch Null geteilt"
//                 },
//                 "Warnings":{
//                     "Caution": "Achtung"
//                 },
//                 "UI":{
//                     "LabelOne": "Eins",
//                     "LabelTwo": "Zwei",
//                     "Login": {
//                         "LabelUserName": "Benutzer",
//                         "LabelPassword": "Passwort"
//                     }
//                 }
//             }
//             """;        
//         this._filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TestLocale.json");
//         File.WriteAllText(this._filePath, localTestData);
//     }
//     
//     public override SourceText GetText(CancellationToken cancellationToken = default) 
//         => SourceText.From(File.ReadAllText(this._filePath));
//
//     public override string Path => this._filePath;
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
//         var generator = new LocalizeCodeGenerator();
//         GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], [new My()]);
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
// }