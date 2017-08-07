using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Raven.CodeAnalysis.ValueTuple
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ValueTupleAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.MethodDeclaration);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptors.ValueTuple);

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var mds = (MethodDeclarationSyntax)context.Node;
            var rs = mds.ReturnType;

            if (rs.IsKind(SyntaxKind.TupleType) == false)
                return;

            var tts = (TupleTypeSyntax)rs;
            foreach (var ttse in tts.Elements)
            {
                var identifier = ttse.Identifier;
                if (identifier.IsKind(SyntaxKind.None))
                    continue;

                var text = identifier.Text;
                if (string.IsNullOrEmpty(text) || char.IsLower(text[0]) == false)
                    continue;

                ReportDiagnostic(context, rs);
                return;
            }
        }

        private static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            CSharpSyntaxNode syntaxNode)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ValueTuple, syntaxNode.GetLocation()));
        }
    }
}