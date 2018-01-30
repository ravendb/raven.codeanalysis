using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.CodeAnalysis.TaskCompletionSource;
using TestHelper;

namespace Raven.CodeAnalysis.Test
{
    [TestClass]
    public class TaskCompletionSourceTests : CodeFixVerifier
    {
        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            const string input = @"
class C
{
	private void Method1() { var tcs = new TaskCompletionSource<string>(TaskContinuationOptions.RunContinuationsAsynchronously); }

    private void Method2() { var tcs = new System.Threading.Tasks.TaskCompletionSource<string>(null, System.Threading.Tasks.TaskContinuationOptions.RunContinuationsAsynchronously); }

	private void Method3() { var tcs = new TaskCompletionSource<string>(TaskContinuationOptions.RunContinuationsAsynchronously | TaskContinuationOptions.AttachedToParent); }
}";
            VerifyCSharpDiagnostic(input);
        }

        [TestMethod]
        public void TestMethod2()
        {
            const string input = @"
class C
{
	private void Method1() { var tcs = new TaskCompletionSource<string>(); }

    private void Method2() { var tcs = new System.Threading.Tasks.TaskCompletionSource<string>(); }

	private void Method3() { var tcs = new TaskCompletionSource<string>(TaskContinuationOptions.AttachedToParent); }
}";
            VerifyCSharpDiagnostic(
                input,
                new DiagnosticResult
                {
                    Id = DiagnosticIds.TaskCompletionSourceMustHaveRunContinuationsAsynchronouslySet,
                    Message = "TaskCompletionSource must have TaskCreationOptions.RunContinuationsAsynchronously set",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 4, 37)
                    }
                },
                new DiagnosticResult
                {
                    Id = DiagnosticIds.TaskCompletionSourceMustHaveRunContinuationsAsynchronouslySet,
                    Message = "TaskCompletionSource must have TaskCreationOptions.RunContinuationsAsynchronously set",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 6, 40)
                    }
                },
                new DiagnosticResult
                {
                    Id = DiagnosticIds.TaskCompletionSourceMustHaveRunContinuationsAsynchronouslySet,
                    Message = "TaskCompletionSource must have TaskCreationOptions.RunContinuationsAsynchronously set",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 37)
                    }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TaskCompletionSourceMustHaveRunContinuationsAsynchronouslySetAnalyzer();
        }
    }
}