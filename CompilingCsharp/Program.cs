using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Reflection;

namespace PrintingSyntaxTree
{
    class Program
    {
        static void Main(string[] args)
        {            
            var tree = SyntaxFactory.ParseSyntaxTree(
                "namespace DemoNamespace "                                  + 
                "{ "                                                        +    
                "   public class Printer"                                   + 
                "   { "                                                     +
                "       public void Print() "                               + 
                "       { "                                                 +
                "           System.Console.WriteLine(\"O TDC é Massa!\"); " +
                "       }  "                                                + 
                "   } "                                                     + 
                "}");

            var compilation = CSharpCompilation.Create(
                "MyDemo",
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] { tree },
                references: new[] { new MetadataFileReference(typeof(object).Assembly.Location) });

            // Generate the assembly into a memory stream
            var memStream = new MemoryStream();
            compilation.Emit(memStream);

            var assembly = Assembly.Load(memStream.GetBuffer());
            dynamic instance = Activator.CreateInstance(assembly.GetTypes().First());
            instance.Print();
        }
    }
}
