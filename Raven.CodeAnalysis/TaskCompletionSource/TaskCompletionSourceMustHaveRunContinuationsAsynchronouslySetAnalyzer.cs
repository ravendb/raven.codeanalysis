using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Raven.CodeAnalysis.TaskCompletionSource
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class TaskCompletionSourceMustHaveRunContinuationsAsynchronouslySetAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string TaskCompletionSourceName = nameof(TaskCompletionSource);

        private static readonly string RunContinuationsAsynchronouslyName = nameof(TaskContinuationOptions.RunContinuationsAsynchronously);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.TaskCompletionSourceMustHaveRunContinuationsAsynchronouslySet);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var objectCreationExpressionSyntax = (ObjectCreationExpressionSyntax)context.Node;
            var genericNameSyntax = objectCreationExpressionSyntax.Type as GenericNameSyntax;
            if (genericNameSyntax == null)
            {
                var qualifiedNameSyntax = objectCreationExpressionSyntax.Type as QualifiedNameSyntax;
                if (qualifiedNameSyntax == null)
                    return;

                genericNameSyntax = qualifiedNameSyntax.Right as GenericNameSyntax;
                if (genericNameSyntax == null)
                    return;
            }

            if (string.Equals(genericNameSyntax.Identifier.Text, TaskCompletionSourceName) == false)
                return;

            var arguments = objectCreationExpressionSyntax.ArgumentList;
            foreach (var argument in arguments.Arguments)
            {
                if (IsRunContinuationsAsynchronously(argument.Expression))
                    return;
            }

            ReportDiagnostic(context, objectCreationExpressionSyntax);
        }

        private static bool IsRunContinuationsAsynchronously(ExpressionSyntax expression)
        {
            var memberAccessExpressionSyntax = expression as MemberAccessExpressionSyntax;
            if (memberAccessExpressionSyntax == null)
            {
                var binaryExpressionSyntax = expression as BinaryExpressionSyntax;
                if (binaryExpressionSyntax == null)
                    return false;

                if (IsRunContinuationsAsynchronously(binaryExpressionSyntax.Left))
                    return true;

                if (IsRunContinuationsAsynchronously(binaryExpressionSyntax.Right))
                    return true;

                return false;
            }

            var name = memberAccessExpressionSyntax.Name.ToString();
            if (name.Contains(RunContinuationsAsynchronouslyName) == false)
                return false;

            if (Enum.TryParse(name, true, out TaskContinuationOptions options) == false)
                return false;

            if (options.HasFlag(TaskContinuationOptions.RunContinuationsAsynchronously))
                return true;

            return false;
        }

        private static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            CSharpSyntaxNode syntaxNode)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.TaskCompletionSourceMustHaveRunContinuationsAsynchronouslySet, syntaxNode.GetLocation()));
        }
    }
}