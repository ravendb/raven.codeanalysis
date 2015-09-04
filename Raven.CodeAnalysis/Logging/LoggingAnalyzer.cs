using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Raven.CodeAnalysis.Logging
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	internal class LoggingAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.Logging);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.ExpressionStatement);
		}

		private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
		{
			var expressionStatementSyntax = (ExpressionStatementSyntax)context.Node;

			var invocationExpressionSyntax =
				expressionStatementSyntax
				.ChildNodes()
				.FirstOrDefault(x => x.IsKind(SyntaxKind.InvocationExpression)) as InvocationExpressionSyntax;

			if (invocationExpressionSyntax == null)
				return;

			var expression = invocationExpressionSyntax.Expression.ToString();
			if (expression.EndsWith(".Debug") == false && expression.EndsWith(".DebugException") == false)
				return;

			var methodSymbol = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax.Expression).Symbol as IMethodSymbol;
			if (methodSymbol == null)
				return;

			if (methodSymbol.ReceiverType.ContainingNamespace.ToDisplayString() != "Raven.Abstractions.Logging")
				return;

			if (methodSymbol.ReceiverType.Name != "ILog")
				return;

			if (methodSymbol.Name != "Debug" && methodSymbol.Name != "DebugException")
				return;

			var ifStatementSyntax = invocationExpressionSyntax
				.Ancestors(ascendOutOfTrivia: false)
				.FirstOrDefault(x => x.IsKind(SyntaxKind.IfStatement)) as IfStatementSyntax;

			if (ifStatementSyntax == null)
			{
				ReportDiagnostic(context, expressionStatementSyntax);
				return;
			}

			SyntaxNode[] nodesToCheck;
			if (ifStatementSyntax.Condition.IsKind(SyntaxKind.SimpleMemberAccessExpression))
			{
				nodesToCheck = new SyntaxNode[] { ifStatementSyntax.Condition };
			}
			else
			{
				nodesToCheck = ifStatementSyntax
					.Condition
					.ChildNodes()
					.Where(x => x.IsKind(SyntaxKind.SimpleMemberAccessExpression))
					.ToArray();
			}

			foreach (var nodeToCheck in nodesToCheck)
			{
				var ifStatementConditionSymbol = context.SemanticModel.GetSymbolInfo(nodeToCheck).Symbol as IPropertySymbol;
				if (ifStatementConditionSymbol == null)
					continue;

				if (ifStatementConditionSymbol.Name == "IsDebugEnabled")
					return;
			}

			ReportDiagnostic(context, expressionStatementSyntax);
		}

		private static void ReportDiagnostic(
			SyntaxNodeAnalysisContext context,
			CSharpSyntaxNode syntaxNode)
		{
			context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Logging, syntaxNode.GetLocation()));
		}
	}
}