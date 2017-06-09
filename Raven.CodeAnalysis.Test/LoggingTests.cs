using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

using Raven.CodeAnalysis.Logging;

namespace Raven.CodeAnalysis.Test
{
	[TestClass]
	public class LoggingTests : CodeFixVerifier
	{

		//No diagnostics expected to show up
		[TestMethod]
		public void TestMethod1()
		{
			const string input = @"
namespace Raven.Abstractions.Logging {
	interface ILog {
		void Debug(string message);
		bool IsDebugEnabled { get; }
	}

	public static class LogManager
	{
		public static ILog GetCurrentClassLogger() { };
	}
}

class C
{
	private static Raven.Abstractions.Logging.ILog Logger = Raven.Abstractions.Logging.LogManager.GetCurrentClassLogger();
    void M()
    {
		Logger.Debug(""cba"");
    }
}";

			VerifyCSharpDiagnostic(input, new DiagnosticResult
			{
				Id = DiagnosticIds.Logging,
				Message = "Wrap Debug and DebugException with IsDebugEnabled condition",
				Severity = DiagnosticSeverity.Error,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 19, 3)
				}
			});
		}

		[TestMethod]
		public void TestMethod2()
		{
			const string input = @"
namespace Raven.Abstractions.Logging {
	interface ILog {
		void Debug(string message);
		bool IsDebugEnabled { get; }
	}

	public static class LogManager
	{
		public static ILog GetCurrentClassLogger() { };
	}
}

class C
{
	private static Raven.Abstractions.Logging.ILog Logger = Raven.Abstractions.Logging.LogManager.GetCurrentClassLogger();
    void M()
    {
		Logger.Debug(""cba"");
    }
}";

			const string output = @"
namespace Raven.Abstractions.Logging {
	interface ILog {
		void Debug(string message);
		bool IsDebugEnabled { get; }
	}

	public static class LogManager
	{
		public static ILog GetCurrentClassLogger() { };
	}
}

class C
{
	private static Raven.Abstractions.Logging.ILog Logger = Raven.Abstractions.Logging.LogManager.GetCurrentClassLogger();
    void M()
    {
        if (Logger.IsDebugEnabled)
            Logger.Debug(""cba"");
    }
}";
			VerifyCSharpFix(input, output);
		}

#if NET45
        protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new LoggingCodeFix();
		}
#endif

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new LoggingAnalyzer();
		}
	}
}