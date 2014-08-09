using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CalculatingLinesOfCode
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
                "       public void PrintTDC() " +                                      Environment.NewLine + 
                "       { " +                                                           Environment.NewLine +
                "           try " +                                                     Environment.NewLine +
                "           { " +                                                       Environment.NewLine +
                "               System.Console.WriteLine(\"O TDC é Massa!\"); " +       Environment.NewLine +
                "           } " +                                                       Environment.NewLine +
                "           catch() " +                                                 Environment.NewLine +
                "           {}" +                                                       Environment.NewLine +
                "       }  " +
                "       public void PrintRoslyn() " +                                   Environment.NewLine +
                "       { " +                                                           Environment.NewLine +
                "           System.Console.WriteLine(\" O Roslyn também é Massa! \")" + Environment.NewLine +
                "       }  " +
                "   } " +
                "}");

            var walker = new LinesOfCodeWalker();
            walker.Visit(tree.GetRoot());
            walker.Methods.ForEach(Console.Write);
        }
    }

    public class LinesOfCodeWalker : SyntaxWalker
    {
        public List<MethodInfo> Methods { get; set; }

        public LinesOfCodeWalker()
        {
            Methods = new List<MethodInfo>();
        }

        public override void Visit(SyntaxNode node)
        {
            if (node.CSharpKind() == SyntaxKind.MethodDeclaration)
            {
                var methodDeclaration = node as MethodDeclarationSyntax;
                var lineSpan = node.SyntaxTree.GetLocation(node.Span).GetLineSpan();
                var startLine = lineSpan.StartLinePosition.Line;
                var endLine = lineSpan.EndLinePosition.Line;

                Methods.Add(new MethodInfo(methodDeclaration.Identifier.Text, endLine - startLine));                
            }

            base.Visit(node);
        }

        public class MethodInfo
        {
            public string Name { get; set; }
            public int LinesOfCode { get; set; }

            public MethodInfo(string name, int linesOfCode)
            {
                Name = name;
                LinesOfCode = linesOfCode;
            }

            public override string ToString()
            {
                return string.Format("{0}: {1}\n", Name, LinesOfCode);
            }
        }
    }
}
