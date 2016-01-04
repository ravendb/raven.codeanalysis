using System.Collections.Immutable;
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
            if (catchStatement?.Block?.Statements.Count != 0) return;
            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.EmptyOrJustLoggingExceptionHandler, catchStatement.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
