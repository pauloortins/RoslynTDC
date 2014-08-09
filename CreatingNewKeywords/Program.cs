using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CreatingNewKeywords
{
    class Program
    {
        /*
           Estamos interessados em criar uma palavra chave que indica que determinado bloco de codigo
           tenha o seu tempo de execucao
        */

        static void Main(string[] args)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
                "namespace DemoNamespace " +
                "{ " +
                "   public class Printer" +
                "   { " +
                "       public void Print() " +
                "       { " +
                "           measureTime " +
                "           { " +
                "               System.Threading.Thread.Sleep(1000);" +
                "               System.Console.WriteLine(\"O TDC é Massa!\"); " +
                "           } " +
                "       }  " +
                "   } " +
                "}");

            var newRoot = new MeasureTimeRewriter().Visit(tree.GetRoot());

            var measureTime = newRoot.DescendantNodes()
                .OfType<ExpressionStatementSyntax>()
                .Where(x => x.ChildNodes().First().GetText().ToString().Trim() == "measureTime");
            
            newRoot = measureTime.ToList().Aggregate(newRoot, (root, x) 
                => root.RemoveNode(x, SyntaxRemoveOptions.KeepNoTrivia));            

            var compilation = CSharpCompilation.Create(
                "MyDemo",
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] { newRoot.SyntaxTree },
                references: new[] { new MetadataFileReference(typeof(object).Assembly.Location) });

            // Generate the assembly into a memory stream
            var memStream = new MemoryStream();
            compilation.Emit(memStream);

            var assembly = Assembly.Load(memStream.GetBuffer());
            dynamic instance = Activator.CreateInstance(assembly.GetTypes().First());
            instance.Print();
        }
    }

    public class MeasureTimeRewriter : CSharpSyntaxRewriter
    {
        private BlockSyntax _blockToEdit;

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            // Just locate the block to me measured
            if (node.GetText().ToString().Trim() == "measureTime")
            {
                var block = (from child in node.Parent.Parent.ChildNodes()
                             where child.CSharpKind() == SyntaxKind.Block
                             select child).FirstOrDefault();

                _blockToEdit = block as BlockSyntax;
            }

            return base.VisitIdentifierName(node);
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {            
            if (node == _blockToEdit)
            {                
                var printTimeStatement = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        GenerateExpressionSyntax("System", "Console", "WriteLine"),
                        SyntaxFactory.ArgumentList(
                            arguments: SyntaxFactory.SeparatedList(
                                new List<ArgumentSyntax>() {
                                    SyntaxFactory.Argument(
                                        expression: 
                                        GenerateExpressionSyntax("System","DateTime","Now"))}))));

                var syntaxList = new SyntaxList<SyntaxNode>();
                syntaxList = syntaxList.Add(printTimeStatement);

                foreach (var stmt in node.Statements)
                {
                    syntaxList = syntaxList.Add(stmt);
                }
                
                syntaxList = syntaxList.Add(printTimeStatement);

                return node.WithStatements(syntaxList);
            }

            return base.VisitBlock(node);
        }

        public ExpressionSyntax GenerateExpressionSyntax(params string[] str)
        {
            var initialExpression = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(str[0]),
                        name: SyntaxFactory.IdentifierName(str[1]));

            var finalExpression = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                    expression: initialExpression,
                    name: SyntaxFactory.IdentifierName(str[2])
                );

            return finalExpression;
        }
    }
}
