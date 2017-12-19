using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.CodeAnalysis.CancellationToken;
using Raven.CodeAnalysis.ConfigureAwait;

using TestHelper;

namespace Raven.CodeAnalysis.Test
{
	[TestClass]
	public class CancellationTokenTests : CodeFixVerifier
	{

		//No diagnostics expected to show up
		[TestMethod]
		public void TestMethod1()
		{
			const string input = @"
class C
{
	private Task Method1() { return null; }

    private Task Method2(CancellationToken token) { return null; }
}";
			VerifyCSharpDiagnostic(input);
		}

		[TestMethod]
		public void TestMethod2()
		{
			const string input = @"
class C
{
	private Task Method1() { return null; }

    private Task Method2(CancellationToken token, string name) { return null; }
}";

			VerifyCSharpDiagnostic(input, new DiagnosticResult
			{
				Id = DiagnosticIds.CancellationTokenMustBeLastArgument,
				Message = "CancellationToken must be a last argument",
				Severity = DiagnosticSeverity.Error,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 6, 5)
				}
			});
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new CancellationTokenMustBeLastArgumentAnalyzer();
		}
	}
}