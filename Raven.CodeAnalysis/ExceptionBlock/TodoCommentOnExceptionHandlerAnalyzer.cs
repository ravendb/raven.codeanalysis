using System;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Raven.CodeAnalysis.ExceptionBlock
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TodoCommentOnExceptionHandlerAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.TodoCommentOnExceptionHandler);

        public override void Initialize(AnalysisContext context) =>
                    context.RegisterSyntaxNodeAction(Analyzer, SyntaxKind.CatchClause);

        private static void Analyzer(SyntaxNodeAnalysisContext context)
        {
            if (context.IsGenerated()) return;
            var catchStatement = (CatchClauseSyntax)context.Node;

            var block = catchStatement.Block;
            if (block == null) return;

            var hasToDoComments =
                block.DescendantTrivia().Any(trivia =>
                    trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) &&
                    IsToDoComment(trivia.ToString())
                    );
              
            if (!hasToDoComments) return;

            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.TodoCommentOnExceptionHandler, catchStatement.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        static bool IsToDoComment(string commentText)
        {
            var trimmedCommentedText = commentText.Substring(2).Trim();
            var prefixes = new[]
            {
                "todo ", "todo:",
                "fixme ", "fixme:",
                "hack ", "hack:",
                "undone ", "undone:"
            };

            return prefixes.Any(prefix =>
                    trimmedCommentedText.StartsWith(
                        prefix, 
                        StringComparison.CurrentCultureIgnoreCase
                    ));
        }
    }
}
