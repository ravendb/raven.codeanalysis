using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Raven.CodeAnalysis.ExceptionBlock
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyOrJustLoggingExceptionHandlerAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.EmptyOrJustLoggingExceptionHandler);

        public override void Initialize(AnalysisContext context) =>
                    context.RegisterSyntaxNodeAction(Analyzer, SyntaxKind.CatchClause);

        private static void Analyzer(SyntaxNodeAnalysisContext context)
        {
            if (context.IsGenerated()) return;
            var catchStatement = (CatchClauseSyntax)context.Node;

            var block = catchStatement.Block;
            if (block == null) return;
            if (HasComments(block)) return;

            var statements = block.Statements;

            if (statements.Count > 2) return;
            if (statements.Count == 2 && !(statements[1] is ReturnStatementSyntax))
                return;

            if (statements.Count > 0 && !IsLogging(statements[0]))
                return;

            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.EmptyOrJustLoggingExceptionHandler, catchStatement.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        
        private static bool IsLogging(StatementSyntax statement)
        {
            var ess = statement as ExpressionStatementSyntax;
            var invocationExpression = ess?.Expression as InvocationExpressionSyntax;
            var arguments = invocationExpression?.ArgumentList?.Arguments;

            var firstArgument = arguments?.FirstOrDefault()?.Expression;

            var firstArgumentLiteral = firstArgument as LiteralExpressionSyntax;
            if (firstArgumentLiteral == null) return false;

            return firstArgumentLiteral.IsKind(SyntaxKind.StringLiteralExpression);
        }

        private static bool HasComments(SyntaxNode block)
        {
            if (block == null) return false;
            var trivias = block.DescendantTrivia();
            return trivias.Any(trivia =>
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                );
        }
    }
}
