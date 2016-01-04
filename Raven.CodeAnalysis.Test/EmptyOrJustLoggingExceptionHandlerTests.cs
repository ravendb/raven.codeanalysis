using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.CodeAnalysis.ExceptionBlock;
using TestHelper;

namespace Raven.CodeAnalysis.Test
{
    [TestClass]
    public class EmptyOrJustLoggingExceptionHandlerTests : CodeFixVerifier
    {

        [TestMethod]
        public void DoNotGenerateDiagnosticWhenRetrowing()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
                throw;
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void DoNotGenerateDiagnosticWhenCommenting()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
                // sample explanation;
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void DoNotGenerateDiagnosticWhenCallingThreadSleep()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
                Thread.Sleep(50);
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void DoNotGenerateDiagnosticWhenDisposingObjects()
        {
            var test = @"
            try
            {
                foo.Dispose();
            }
            catch {}".AsMethodCode();
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void GenerateDiagnosticWhenCatchIsAnEmptyBlock()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test, new DiagnosticResult
            {
                Id = DiagnosticIds.EmptyOrJustLoggingExceptionHandler,
                Message = "This exception should be properly handled",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 14, 13)
                }
            });
        }

        [TestMethod]
        public void GenerateDiagnosticWhenCatchIsLogging()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
                log.Error(""bla bla"", ex);
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test, new DiagnosticResult
            {
                Id = DiagnosticIds.EmptyOrJustLoggingExceptionHandler,
                Message = "This exception should be properly handled",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 14, 13)
                }
            });
        }

        [TestMethod]
        public void GenerateDiagnosticWhenCatchIsLoggingAndReturn()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
                log.Error(""bla bla"", ex);
                return;
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test, new DiagnosticResult
            {
                Id = DiagnosticIds.EmptyOrJustLoggingExceptionHandler,
                Message = "This exception should be properly handled",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 14, 13)
                }
            });
        }


        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EmptyOrJustLoggingExceptionHandlerAnalyzer();
        }
    }
}
