using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Raven.CodeAnalysis.CancellationToken
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class CancellationTokenMustBeLastArgumentAnalyzer : DiagnosticAnalyzer
    {
        private static readonly Type CancellationTokenType = typeof(System.Threading.CancellationToken);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.CancellationTokenMustBeLastArgument);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;
            var parameters = methodDeclarationSyntax.ParameterList.Parameters;
            if (parameters.Count == 0)
                return;

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var type = parameter.Type.ToString();

                if (type != CancellationTokenType.Name && type != CancellationTokenType.FullName)
                    continue;

                if (i == parameters.Count - 1) 
                    continue;

                ReportDiagnostic(context, methodDeclarationSyntax);
                return;
            }
        }

        private static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            CSharpSyntaxNode syntaxNode)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CancellationTokenMustBeLastArgument, syntaxNode.GetLocation()));
        }
    }
}