using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Raven.CodeAnalysis.ConfigureAwait;

using TestHelper;

namespace Raven.CodeAnalysis.Test
{
	[TestClass]
	public class ConfigureAwaitTests : CodeFixVerifier
	{

		//No diagnostics expected to show up
		[TestMethod]
		public void TestMethod1()
		{
			const string input = @"
class C
{
	private Task SthAsync() { return null; }

	async Task M()
    {
		SthAsync();
    }
}";
			VerifyCSharpDiagnostic(input);
		}

		[TestMethod]
		public void TestMethod2()
		{
			const string input = @"
class C
{
	private Task SthAsync() { return null; }

	async Task M()
    {
		await SthAsync();
    }
}";

			VerifyCSharpDiagnostic(input, new DiagnosticResult
			{
				Id = DiagnosticIds.ConfigureAwait,
				Message = "Awaited operations must have ConfigureAwait(false)",
				Severity = DiagnosticSeverity.Error,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 8, 3)
				}
			});
		}

		[TestMethod]
		public void TestMethod3()
		{
			const string input = @"
class C
{
	private Task SthAsync() { return null; }

	async Task M()
    {
		await SthAsync().ConfigureAwait(false);
    }
}";

			VerifyCSharpDiagnostic(input);
		}

		[TestMethod]
		public void TestMethod4()
		{
			const string input = @"
class C
{
	private Task SthAsync() { return null; }

	async Task M()
    {
		await SthAsync().ConfigureAwait(continueOnCapturedContext: false);
    }
}";

			VerifyCSharpDiagnostic(input);
		}

		[TestMethod]
		public void TestMethod5()
		{
			const string input = @"
class C
{
	private Task SthAsync() { return null; }

	async Task M()
    {
		await SthAsync().ConfigureAwait(true);
    }
}";

			VerifyCSharpDiagnostic(input, new DiagnosticResult
			{
				Id = DiagnosticIds.ConfigureAwait,
				Message = "Awaited operations must have ConfigureAwait(false)",
				Severity = DiagnosticSeverity.Error,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 8, 3)
				}
			});
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new ConfigureAwaitAnalyzer();
		}
	}
}