using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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
            
            
            if (expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) == false && expression.IsKind(SyntaxKind.IdentifierName) == false)
                return;

            if (TryGetSymbolOrCandidate(context, expression, out IMethodSymbol methodSymbol) == false)
                return;

            // Using ToDisplayString() is more robust as it includes the full namespace
            if (methodSymbol.ContainingType.ToDisplayString().Equals("Raven.Database.Config.RavenConfiguration", StringComparison.Ordinal) == false ||
                methodSymbol.Name.Equals("GetKey", StringComparison.Ordinal) == false)
                return;

            var argumentList = invocationExpressionSyntax.ArgumentList;

            if (argumentList == null || argumentList.Arguments.Count != 1)
                return;

            if (!(argumentList.Arguments[0].Expression is SimpleLambdaExpressionSyntax simpleLambda))
                return;

            if (!(simpleLambda.Body is MemberAccessExpressionSyntax propertyAccessExpression))
                return;

            var configurationType = methodSymbol.ContainingType;
            
            var propertyIdentifier = propertyAccessExpression.Name;
            var propertyName = propertyIdentifier.Identifier.ValueText;

            var propertySymbol = configurationType.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault();

            // Get the symbol from the entire member access expression (e.g., "x.OrdinaryProperty")
            // This is the key change
            if (propertySymbol == null) 
                return;

            var attributes = propertySymbol.GetAttributes();
            // A more robust check for the attribute by its name
            if (attributes.Any(attr => attr.AttributeClass.Name.Equals("ConfigurationEntryAttribute", StringComparison.Ordinal)))
                return;

            // The diagnostic should be reported on the name of the property
            ReportDiagnostic(context, propertyIdentifier, propertyIdentifier.Identifier.ValueText);
        }

        private static bool TryGetSymbolOrCandidate<TSymbol>(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, out TSymbol result)
            where TSymbol : class, ISymbol
        {
            var info = context.SemanticModel.GetSymbolInfo(expression);

            var symbol = info.Symbol as TSymbol;
            if (symbol == null && info.CandidateSymbols.Length == 1)
            {
                symbol = info.CandidateSymbols[0] as TSymbol;
            }

            result = symbol;
            return symbol != null;
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