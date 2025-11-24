using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Raven.CodeAnalysis.BooleanMethodNegation
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = "Rewrite negated boolean method conditions")]
    internal class BooleanMethodNegationCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.BooleanMethodNegation);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            var syntaxNode = root.FindNode(context.Span, getInnermostNodeForTie: true) as PrefixUnaryExpressionSyntax;
            if (syntaxNode == null)
                return;

            context.RegisterCodeFix(CodeAction.Create(
                    "Rewrite boolean method condition to comparison",
                    token => RewriteAsync(context.Document, syntaxNode, token)),
                context.Diagnostics);
        }

        private static async Task<Document> RewriteAsync(Document document, PrefixUnaryExpressionSyntax logicalNotExpression, System.Threading.CancellationToken token)
        {
            var operand = logicalNotExpression.Operand;
            while (operand is ParenthesizedExpressionSyntax parenthesizedExpressionSyntax)
            {
                operand = parenthesizedExpressionSyntax.Expression;
            }

            var invocationExpressionSyntax = operand as InvocationExpressionSyntax;
            if (invocationExpressionSyntax == null)
                return document;

            var newCondition = SyntaxFactory.BinaryExpression(
                    SyntaxKind.EqualsExpression,
                    invocationExpressionSyntax,
                    SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))
                .WithTriviaFrom(logicalNotExpression);

            var root = await document.GetSyntaxRootAsync(token);
            root = root.ReplaceNode(logicalNotExpression, newCondition);

            return document.WithSyntaxRoot(root);
        }
    }
}