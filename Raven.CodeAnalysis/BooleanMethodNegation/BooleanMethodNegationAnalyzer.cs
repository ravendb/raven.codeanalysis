using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Raven.CodeAnalysis.BooleanMethodNegation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class BooleanMethodNegationAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.BooleanMethodNegation);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.LogicalNotExpression);
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var logicalNotExpressionSyntax = (PrefixUnaryExpressionSyntax)context.Node;

            var operand = logicalNotExpressionSyntax.Operand;
            while (operand is ParenthesizedExpressionSyntax parenthesizedExpressionSyntax)
            {
                operand = parenthesizedExpressionSyntax.Expression;
            }

            if (operand is InvocationExpressionSyntax invocation == false)
                return;

            var semanticModel = context.SemanticModel;
            var methodSymbol = semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;

            if (methodSymbol?.ReturnType?.SpecialType != SpecialType.System_Boolean)
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.BooleanMethodNegation,
                logicalNotExpressionSyntax.GetLocation(),
                methodSymbol.Name));
        }
    }
}