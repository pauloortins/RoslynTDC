using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindingBadTryCatch
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
                "           try " +
                "           { " +
                "               System.Console.WriteLine(\"O TDC é Massa!\"); " +
                "           } " +
                "           catch() " +
                "           { " +
                "           } " + 
                "       }  " +
                "   } " +
                "}");

            new CatchClauseWalker().Visit(tree.GetRoot());
        }
    }

    public class CatchClauseWalker : SyntaxWalker
    {        
        public override void Visit(SyntaxNode node)
        {
            if (node.CSharpKind() == SyntaxKind.CatchClause)
            {
                var catchClauseSyntax = node as CatchClauseSyntax;
                if (catchClauseSyntax.Block.Statements.Count == 0)
                {
                    Console.WriteLine(
                        "Exceção está sendo suprimida. " + 
                        "Tem certeza que você quer fazer isso?");
                }
            }

            base.Visit(node);
        }
    }
}
