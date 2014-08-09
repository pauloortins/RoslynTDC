using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace CatchClause
{
    [ExportCodeFixProvider("CatchClause", LanguageNames.CSharp)]
    public class CodeFixProvider : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { CatchClauseAnalyzer.MakeConstDiagnosticId };
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var diagnosticSpan = diagnostics.First().Location.SourceSpan;            
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<CatchClauseSyntax>().First();
                        
            return new[] { CodeAction.Create("Add throw statement", c => FixCatchClauseAsync(document, declaration, c)) };
        }

        private async Task<Document> FixCatchClauseAsync(Document document, 
            CatchClauseSyntax declaration, 
            CancellationToken cancellationToken)
        {
            var newCatchClause = SyntaxFactory
                        .CatchClause()
                        .WithBlock(
                            SyntaxFactory.Block(statements: SyntaxFactory.ThrowStatement()));

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newCatchClause);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
