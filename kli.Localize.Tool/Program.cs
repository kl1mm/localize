using System.IO;
using System.Linq;
using kli.Localize.Tool.Internal;

namespace kli.Localize.Tool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 2 && File.Exists(args.Last()))
            {
                var codeGenerator = new CodeGenerator();
                var code = codeGenerator.CreateClass(args.First(), args.Last());
                File.WriteAllText(GetOutputFileName(args.Last()), code);
            }
        }

        private static string GetOutputFileName(string sourceFile)
        {
            var fileName = $"{Path.GetFileNameWithoutExtension(sourceFile)}.Generated.cs";
            var basePath = Path.GetDirectoryName(sourceFile);
            return Path.Combine(basePath, fileName);
        }
    }
}
