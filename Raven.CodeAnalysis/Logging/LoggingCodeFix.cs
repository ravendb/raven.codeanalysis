using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Raven.CodeAnalysis.Logging
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = "Wrap Debug and DebugException with IsDebugEnabled")]
	internal class LoggingCodeFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.Logging);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
			var syntaxNode = root.FindNode(context.Span, getInnermostNodeForTie: true) as ExpressionStatementSyntax;
			if (syntaxNode == null)
				return;

			context.RegisterCodeFix(CodeAction.Create("Wrap Debug and DebugException with IsDebugEnabled", token => WrapAsync(context.Document, syntaxNode, token)), context.Diagnostics);
		}

		private static async Task<Document> WrapAsync(Document document, StatementSyntax expressionStatement, CancellationToken token)
		{
			var identifierNameSyntax =
				expressionStatement.DescendantNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierName)) as
				IdentifierNameSyntax;

			if (identifierNameSyntax == null)
				return document;

			var loggerName = identifierNameSyntax.Identifier.Text;

			var expression = SyntaxFactory.ParseExpression(loggerName + ".IsDebugEnabled");
			var ifStatement = SyntaxFactory.IfStatement(expression, expressionStatement);

			var root = await document.GetSyntaxRootAsync(token);
			root = root.ReplaceNode(expressionStatement, ifStatement);

			return document.WithSyntaxRoot(root);
		}
	}
}