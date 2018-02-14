using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Raven.CodeAnalysis.ReturnNotAwaitedTaskInsideUsing
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MustNotReturnTaskInsideUsingStatementAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.MethodDeclaration);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MustNotReturnTaskInsideUsingStatementAnalyzer);

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword))
                return;

            var returnType = methodDeclaration.ReturnType;

            if (returnType is IdentifierNameSyntax identifierNameSyntax)
            {
                if (identifierNameSyntax.Identifier.Text != nameof(Task))
                    return;
            }
            else if (returnType is GenericNameSyntax genericNameSyntax)
            {
                if (genericNameSyntax.Identifier.Text != nameof(Task))
                    return;
            }
            else
            {
                return;
            }

            var body = methodDeclaration.Body;

            if (body == null)
                return;

            var usings = body.Statements.OfKind(SyntaxKind.UsingStatement).ToList();

            if (usings.Count == 0)
                return;

            foreach (var usingStatement in usings)
            {
                var usingStatementSyntax = (UsingStatementSyntax)usingStatement;

                if (usingStatementSyntax.Statement is BlockSyntax usingBlock)
                {
                    var returnStatements = usingBlock.Statements.OfKind(SyntaxKind.ReturnStatement).ToList();

                    if (returnStatements.Count == 0)
                        continue;

                    foreach (var returnStatement in returnStatements)
                    {
                        var returnStatementSyntax = (ReturnStatementSyntax)returnStatement;

                        if (returnStatementSyntax.Expression is AwaitExpressionSyntax)
                            continue;

                        if (returnStatementSyntax.Expression is MemberAccessExpressionSyntax memberAccessSyntax)
                        {
                            if (memberAccessSyntax.Expression is IdentifierNameSyntax ins)
                            {
                                if (ins.Identifier.Text == "Task" && memberAccessSyntax.Name?.Identifier.Text == "CompletedTask")
                                    continue;
                            }
                        }

                        if (returnStatementSyntax.Expression is InvocationExpressionSyntax invocationExpressionSyntax)
                        {
                            if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax invocationMember)
                            {
                                if (invocationMember.Expression is IdentifierNameSyntax ins)
                                {
                                    if (ins.Identifier.Text == "Task" && invocationMember.Name?.Identifier.Text.StartsWith("From") == true)
                                        continue;
                                }
                            }
                        }

                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MustNotReturnTaskInsideUsingStatementAnalyzer, returnStatementSyntax.GetLocation()));
                    }
                }
            }
        }
    }
}