using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FixingBadTryCatch
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

            var newRoot = new CatchClauseRewriter().Visit(tree.GetRoot());
                        
            Console.WriteLine(newRoot.GetText());
        }

        public class CatchClauseRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitCatchClause(CatchClauseSyntax node)
            {
                if (node.Block.Statements.Count == 0)
                {                    
                    return SyntaxFactory
                        .CatchClause()
                        .WithBlock(
                            SyntaxFactory.Block(statements: SyntaxFactory.ThrowStatement()));
                }

                return base.VisitCatchClause(node);                
            }
        }
    }
}
