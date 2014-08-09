using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CatchClause
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("CatchClause", LanguageNames.CSharp)]
    public class CatchClauseAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string MakeConstDiagnosticId = "CatchClause";
        public static readonly DiagnosticDescriptor MakeConstRule = new DiagnosticDescriptor(MakeConstDiagnosticId,
                                                                                             "Throw an Exception",
                                                                                             "You should not hide errors",
                                                                                             "Usage",
                                                                                             DiagnosticSeverity.Warning,
                                                                                             isEnabledByDefault: true);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.CatchClause); } }
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(MakeConstRule); } }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            var catchClauseNode = node as CatchClauseSyntax;
            if (!catchClauseNode.Block.Statements.Any())
            {
                addDiagnostic(Diagnostic.Create(MakeConstRule, node.GetLocation()));
            }
        }
    }
}