using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessingSolutions
{
    class Program
    {
        static void Main(string[] args)
        {            
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            
            Solution originalSolution = workspace.OpenSolutionAsync(@"TestSolution\TestSolution.sln").Result;
            Solution newSolution = originalSolution;

            var documents = GetDocuments(originalSolution);

            foreach (ProjectId projectId in originalSolution.ProjectIds)
            {                
                Project project = newSolution.GetProject(projectId);                

                foreach (DocumentId documentId in project.DocumentIds)
                {             
                    Document document = newSolution.GetDocument(documentId);
                    document = document.WithSyntaxRoot(new CatchClauseRewriter().Visit(document.GetSyntaxRootAsync().Result));

                    Document newDocument = Formatter.FormatAsync(document).Result;                    
                    newSolution = newDocument.Project.Solution;
                }
            }

            if (workspace.TryApplyChanges(newSolution))
            {
                Console.WriteLine("Solution updated.");
            }
            else
            {
                Console.WriteLine("Update failed!");
            }
        }

        public static List<Document> GetDocuments(Solution solution)
        {
            var documents = new List<Document>();

            foreach (ProjectId projectId in solution.ProjectIds)
            {
                Project project = solution.GetProject(projectId);
                foreach (DocumentId documentId in project.DocumentIds)
                {
                    var document = solution.GetDocument(documentId);
                    documents.Add(document);
                }
            }

            return documents;
        }

        public class CatchClauseRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitCatchClause(CatchClauseSyntax node)
            {
                if (node.Block.Statements.Count == 0)
                {
                    return SyntaxFactory.CatchClause().WithBlock(SyntaxFactory.Block(statements: SyntaxFactory.ThrowStatement()));
                }

                return base.VisitCatchClause(node);
            }
        }
    }
}
