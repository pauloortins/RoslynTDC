using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PrintingSyntaxTree
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

            Print(tree.GetRoot());     
        }

        public static void Print(SyntaxNode syntaxNode)
        {
            Console.WriteLine("---- {0} ----", syntaxNode.CSharpKind());
            Console.WriteLine("{0}\n", syntaxNode.GetText());

            syntaxNode.ChildNodes().ToList().ForEach(Print);
        }
    }
}
