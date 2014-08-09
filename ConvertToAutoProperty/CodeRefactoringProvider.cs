using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ExtractIf
{
    [ExportCodeRefactoringProvider("ExtractIf", LanguageNames.CSharp)]
    public class CodeRefactoringProvider : ICodeRefactoringProvider
    {
        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(textSpan.Start);
            if (token.Parent == null)
            {
                return null;
            }

            var ifDeclaration = token.Parent.FirstAncestorOrSelf<IfStatementSyntax>();
            if (ifDeclaration == null)
                return null;

            return new[] { CodeAction.Create("Extract If", (c) => ExtractIfAsync(document, ifDeclaration, c)) };
        }

        private async Task<Document> ExtractIfAsync(Document document, IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var expressionVisitor = new BinaryExpressionWalker();
            expressionVisitor.Visit(ifStatement);
            
            var newRoot = new MethodDeclarationRewriter(expressionVisitor.Expressions).Visit(root);           
            return document.WithSyntaxRoot(newRoot);
        }

        public class BinaryExpressionWalker : SyntaxWalker
        {
            public List<BinaryExpressionSyntax> Expressions { get; set; }

            public BinaryExpressionWalker()
            {
                Expressions = new List<BinaryExpressionSyntax>();
            }

            public override void Visit(SyntaxNode node)
            {
                var expression = node as BinaryExpressionSyntax;
                if (expression != null && expression.IsLeaf())
                {
                    Expressions.Add(expression);
                }

                base.Visit(node);
            }
        }

        public class MethodDeclarationRewriter : CSharpSyntaxRewriter
        {
            public List<BinaryExpressionSyntax> Expressions { get; set; }
            public MethodDeclarationRewriter(List<BinaryExpressionSyntax> expressions)
            {
                Expressions = expressions;
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                var ifs = node.DescendantNodes().OfType<IfStatementSyntax>();
                foreach (var ifStatement in ifs)
                {
                    var localDeclarations = Expressions
                        .Where(x => ifStatement.DescendantNodes().OfType<BinaryExpressionSyntax>().Any(y => y.ToString() == x.ToString()))
                        .Select(x => x.CreateLocalDeclaration().WithLeadingTrivia(ifStatement.GetLeadingTrivia())).ToList();

                    node = node.InsertNodesBefore(ifStatement, localDeclarations);
                    var newIf = (IfStatementSyntax)new BinaryExpressionRewriter(Expressions).Visit(ifStatement);
                    var oldIf = node.DescendantNodes().OfType<IfStatementSyntax>().First(x => x.ToString() == ifStatement.ToString());
                    node = node.ReplaceNode(oldIf, newIf);
                }

                return node;
            }            
        }

        public class BinaryExpressionRewriter : CSharpSyntaxRewriter
        {
            public List<BinaryExpressionSyntax> Expressions { get; set; }

            public BinaryExpressionRewriter(List<BinaryExpressionSyntax> expressions)
            {
                Expressions = expressions;
            }

            public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                return ToIdentifier(node);
            }

            private ExpressionSyntax ToIdentifier(BinaryExpressionSyntax node)
            {
                if (node.IsLeaf() && Expressions.Any(x => x.ToString() == node.ToString()))
                {
                    var variableName = node.Name();
                    var identifier = SyntaxFactory.IdentifierName(variableName);
                    return identifier;
                }

                if (node.Left is BinaryExpressionSyntax)
                {
                    node = node.WithLeft(ToIdentifier((BinaryExpressionSyntax)node.Left).WithTrailingTrivia(SyntaxFactory.Whitespace(" ")));
                }

                if (node.Right is BinaryExpressionSyntax)
                {
                    node = node.WithRight(ToIdentifier((BinaryExpressionSyntax)node.Right));
                }

                return node;
            } 
        }
    }

    public static class Extensions
    {
        public static string Name(this BinaryExpressionSyntax node)
        {
            var left = node.Left.ToString();
            var oper = node.OperatorToken.CSharpKind().ToString();
            var right = node.Right.ToString();

            return left + oper.Replace("Token", string.Empty) + right;
        }

        public static bool IsLeaf(this BinaryExpressionSyntax node)
        {
            return !(node.Left is BinaryExpressionSyntax) && !(node.Right is BinaryExpressionSyntax);
        }

        public static LocalDeclarationStatementSyntax CreateLocalDeclaration(this BinaryExpressionSyntax node)
        {
            var localDeclaration = SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName("var").WithTrailingTrivia(SyntaxFactory.Whitespace(" ")),
                            SyntaxFactory.SeparatedList(
                                new List<VariableDeclaratorSyntax> {
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier(node.Name()), null, 
                                    initializer: SyntaxFactory.EqualsValueClause(node).WithTrailingTrivia())
                                }
                ))).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));

            return localDeclaration;
        }
    }
}