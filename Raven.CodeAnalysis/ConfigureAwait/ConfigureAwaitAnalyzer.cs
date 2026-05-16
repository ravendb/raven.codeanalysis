using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Raven.CodeAnalysis.ConfigureAwait
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	internal class ConfigureAwaitAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.ConfigureAwait);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.AwaitExpression);
		}

		private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
		{
			var awaitExpressionSyntax = (AwaitExpressionSyntax)context.Node;
			var invocationExpressionSyntax = awaitExpressionSyntax.Expression as InvocationExpressionSyntax;
			var awaitedExpression = invocationExpressionSyntax?.Expression as MemberAccessExpressionSyntax;

			if (awaitedExpression?.Name.Identifier.Text == "ConfigureAwait")
			{
                // safely get the first argument, or null if the user hasn't typed it yet
                var argument = invocationExpressionSyntax.ArgumentList.Arguments.FirstOrDefault()?.Expression;
                if (argument != null)
                {
                    var configureAwaitIsExplicitTrue = argument.IsKind(SyntaxKind.TrueLiteralExpression);
                    if (configureAwaitIsExplicitTrue == false)
                        return;
                }
            }

			ReportDiagnostic(context, awaitExpressionSyntax);
		}

		private static void ReportDiagnostic(
			SyntaxNodeAnalysisContext context,
			CSharpSyntaxNode syntaxNode)
		{
			context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ConfigureAwait, syntaxNode.GetLocation()));
		}
	}
}