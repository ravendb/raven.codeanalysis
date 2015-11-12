using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Raven.CodeAnalysis.Configuration
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class GetConfigurationEntryKeyAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptors.GetConfigurationEntryKey);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;

            var expression = invocationExpressionSyntax.Expression;
            
            if (expression == null)
                return;

            if (expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) == false && expression.IsKind(SyntaxKind.IdentifierName) == false)
                return;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(expression).Symbol as IMethodSymbol;

            if (methodSymbol == null)
                return;

            if (methodSymbol.ReceiverType.Name.Equals("RavenConfiguration", StringComparison.Ordinal) == false || methodSymbol.Name.Equals("GetKey", StringComparison.Ordinal) == false)
                return;

            var argumentList = invocationExpressionSyntax.ArgumentList;

            if (argumentList == null || argumentList.Arguments.Count != 1)
                return;

            var simpleLambda = argumentList.Arguments[0].Expression as SimpleLambdaExpressionSyntax;

            var syntaxToken = simpleLambda?.Body as MemberAccessExpressionSyntax;

            var propertyIdentifier = syntaxToken?.ChildNodes().OfType<IdentifierNameSyntax>().LastOrDefault();

            if (propertyIdentifier == null)
                return;

            var propertySymbol = context.SemanticModel.GetSymbolInfo(propertyIdentifier);
            if (propertySymbol.Symbol == null)
                return;

            var attributes = propertySymbol.Symbol.GetAttributes();
            if (attributes.Any(x => x.AttributeClass.ToString().Contains("ConfigurationEntryAttribute")))
                return;

            ReportDiagnostic(context, propertyIdentifier, propertyIdentifier.Identifier.ValueText);
        }

        private static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            CSharpSyntaxNode syntaxNode,
            string propertyName)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GetConfigurationEntryKey, syntaxNode.GetLocation(), propertyName));
        }
    }
}