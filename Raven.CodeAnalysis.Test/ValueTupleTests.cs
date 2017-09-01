using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.CodeAnalysis.ValueTuple;
using TestHelper;

namespace Raven.CodeAnalysis.Test
{
    [TestClass]
    public class ValueTupleTests : CodeFixVerifier
    {
        //No diagnostics expected to show up
        [TestMethod]
        public void ValueTupleValid()
        {
            const string input = @"
class C
{
	private void M1() { }

    private Task M2() { }

	private (long, string) M3() { return (0, null); }

    private (long T1, string T2) M4() { return (0, null); }
}";
            VerifyCSharpDiagnostic(input);
        }

        [TestMethod]
        public void ValueTupleInvalid()
        {
            const string input = @"
class C
{
    private (long T1, string t2) M4() { return (0, null); }
}";
            VerifyCSharpDiagnostic(input, new DiagnosticResult
            {
                Id = DiagnosticIds.ValueTupleVariablesMustBeUppercase,
                Message = "Use PascalCase in named ValueTuples",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 13)
                }
            });
        }

        [TestMethod]
        public void ValueTupleAsParameterInvalid()
        {
            const string input = @"
class C
{
    private void M4((long T1, string t2) t) { }
}";
            VerifyCSharpDiagnostic(input, new DiagnosticResult
            {
                Id = DiagnosticIds.ValueTupleVariablesMustBeUppercase,
                Message = "Use PascalCase in named ValueTuples",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 4, 21)
                }
            });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ValueTupleAnalyzer();
        }
    }
}