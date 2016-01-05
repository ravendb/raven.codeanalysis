using System.Collections.Immutable;
using System.Linq;

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
            if (statements.Count == 2 && !(statements[1] is ReturnStatementSyntax) && !(statements[1] is BreakStatementSyntax))
                return;

            if (statements.Count > 0 && !IsLogging(statements[0]))
                return;

            if (statements.Count == 0 && AreTryingToDispose(catchStatement.Parent as TryStatementSyntax))
                return;
            
            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.EmptyOrJustLoggingExceptionHandler, catchStatement.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        private static bool AreTryingToDispose(TryStatementSyntax tryStatement)
        {
            var statements = tryStatement?.Block?.Statements;
            if (statements?.Count != 1) return false;

            var statement = statements?.FirstOrDefault();

            return
                AreTryingToDisposeDirectly(statement as ExpressionStatementSyntax) ||
                AreTryingToDisposeCheckingIfObjectIsNotNull(statement as IfStatementSyntax);
        }

        private static bool AreTryingToDisposeDirectly(ExpressionStatementSyntax statement)
        {
            var invocation = statement?.Expression as InvocationExpressionSyntax;
            var memberAccess = invocation?.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null) return false;

            return memberAccess.Name.ToString() == "Dispose";
        }

        private static bool AreTryingToDisposeCheckingIfObjectIsNotNull(IfStatementSyntax ifStatement)
        {
            if (ifStatement == null) return false;

            if (ifStatement.Else != null) return false;
            
            var statement = ifStatement.Statement as ExpressionStatementSyntax;
            var invocation = statement?.Expression as InvocationExpressionSyntax;
            var memberAccess = invocation?.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null) return false;

            return memberAccess.Name.ToString() == "Dispose";
        }


        private static bool IsLogging(StatementSyntax statement)
        {
            var ess = statement as ExpressionStatementSyntax;
            var invocationExpression = ess?.Expression as InvocationExpressionSyntax;
            var arguments = invocationExpression?.ArgumentList?.Arguments;

            var firstArgument = arguments?.FirstOrDefault()?.Expression;

            var firstArgumentLiteral = firstArgument as LiteralExpressionSyntax;
            return firstArgumentLiteral != null && firstArgumentLiteral.IsKind(SyntaxKind.StringLiteralExpression);
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
