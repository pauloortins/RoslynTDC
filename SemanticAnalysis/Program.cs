using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
                "namespace DemoNamespace " +
                "{ " +
                "   public class Printer" +
                "   { " +
                "       public void Print() " +
                "       { " +
                "           System.Console.WriteLine(\"O TDC é Massa!\"); " +
                "       }  " +
                "   } " +
                "}");

            var writeLine = tree.GetRoot().DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .First(x => x.Identifier.ValueText == "WriteLine");
            var compilation = CSharpCompilation.Create("Demo")
                 .AddReferences(
                 new MetadataFileReference(
                 typeof(Console).Assembly.Location))
                 .AddSyntaxTrees(tree);

            var info = compilation.GetSemanticModel(tree).GetSymbolInfo(writeLine);
            Console.WriteLine(info.Symbol.ToString());
            // System.Console.WriteLine(string)
        }
    }
}
