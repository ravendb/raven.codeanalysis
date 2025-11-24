using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.CodeAnalysis.BooleanMethodNegation;
using TestHelper;

namespace Raven.CodeAnalysis.Test
{
    [TestClass]
    public class BooleanMethodNegationTests : CodeFixVerifier
    {
        [TestMethod]
        public void ShouldReportDiagnosticOnNegatedBooleanMethod()
        {
            const string input = @"
class C
{
    private bool HasPermission()
    {
        return false;
    }

    void M()
    {
        if (!HasPermission())
        {
        }
    }
}";
            VerifyCSharpDiagnostic(input, new DiagnosticResult
            {
                Id = DiagnosticIds.BooleanMethodNegation,
                Message = "Negated boolean method 'HasPermission' conditions should be rewritten as HasPermission(...) == false",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 13)
                }
            });
        }

        [TestMethod]
        public void ShouldReportDiagnosticOnNegatedBooleanMethodWithArguments()
        {
            const string input = @"
class C
{
    private bool IsValid(int number)
    {
        return number > 0;
    }

    void M()
    {
        if (!IsValid(1))
        {
        }
    }
}";
            VerifyCSharpDiagnostic(input, new DiagnosticResult
            {
                Id = DiagnosticIds.BooleanMethodNegation,
                Message = "Negated boolean method 'IsValid' conditions should be rewritten as IsValid(...) == false",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 13)
                }
            });
        }

        [TestMethod]
        public void ShouldNotReportDiagnosticOnNonNegatedBooleanMethod()
        {
            const string input = @"
class C
{
    private bool HasPermission()
    {
        return false;
    }

    void M()
    {
        if (HasPermission())
        {
        }
    }
}";
            VerifyCSharpDiagnostic(input);
        }

        [TestMethod]
        public void ShouldNotReportDiagnosticOnComparisonToFalse()
        {
            const string input = @"
class C
{
    private bool IsValid(int number)
    {
        return number > 0;
    }

    void M()
    {
        if (IsValid(1) == false)
        {
        }
    }
}";
            VerifyCSharpDiagnostic(input);
        }

        [TestMethod]
        public void ShouldRewriteNegatedBooleanMethodCondition()
        {
            const string input = @"
class C
{
    private bool HasPermission()
    {
        return false;
    }

    void M()
    {
        if (!HasPermission())
        {
        }
    }
}";
            const string output = @"
class C
{
    private bool HasPermission()
    {
        return false;
    }

    void M()
    {
        if (HasPermission() == false)
        {
        }
    }
}";
            VerifyCSharpFix(input, output);
        }

        [TestMethod]
        public void ShouldRewriteNegatedBooleanMethodConditionWithArguments()
        {
            const string input = @"
class C
{
    private bool IsValid(int number)
    {
        return number > 0;
    }

    void M()
    {
        if (!IsValid(1))
        {
        }
    }
}";
            const string output = @"
class C
{
    private bool IsValid(int number)
    {
        return number > 0;
    }

    void M()
    {
        if (IsValid(1) == false)
        {
        }
    }
}";
            VerifyCSharpFix(input, output);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new BooleanMethodNegationCodeFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new BooleanMethodNegationAnalyzer();
        }
    }
}