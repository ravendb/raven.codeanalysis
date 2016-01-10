using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.CodeAnalysis.ExceptionBlock;
using TestHelper;

namespace Raven.CodeAnalysis.Test
{
    [TestClass]
    public class TodoCommentOnExceptionHandlerTests : CodeFixVerifier
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
        public void DoNotGenerateDiagnosticWithRegularComments()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
                // this is not a TODO comment;
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
                // this is a properly commented exception handler
                Thread.Sleep(50);
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void DoNotGenerateDiagnosticWhenCommentStartsWithToDoButItsNotAnToDoComment()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
                // todos os comentarios em portugues sao importates
                Thread.Sleep(50);
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void DoNotGenerateDiagnosticWithEmptyExceptionHandler()
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
        public void GenerateDiagnosticWithToDoComment()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
                log.Error(""This was an error"", ex); 
                // TODO: I should write a good comment here
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test, new DiagnosticResult
            {
                Id = DiagnosticIds.TodoCommentOnExceptionHandler,
                Message = "ToDo comments should be resolved and this exception should be properly handled",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 14, 13)
                }
            });
        }

        [TestMethod]
        public void GenerateDiagnosticWithHackComment()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
                log.Error(""This was an error"", ex); 
                // HACK: I should write a good comment here
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test, new DiagnosticResult
            {
                Id = DiagnosticIds.TodoCommentOnExceptionHandler,
                Message = "ToDo comments should be resolved and this exception should be properly handled",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 14, 13)
                }
            });
        }

        [TestMethod]
        public void GenerateDiagnosticWithUndoneComment()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
                log.Error(""This was an error"", ex); 
                // UNDONE: I should write a good comment here
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test, new DiagnosticResult
            {
                Id = DiagnosticIds.TodoCommentOnExceptionHandler,
                Message = "ToDo comments should be resolved and this exception should be properly handled",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 14, 13)
                }
            });
        }

        [TestMethod]
        public void GenerateDiagnosticWithFixMeComment()
        {
            var test = @"
            try
            {
                // do something
            }
            catch (Exception ex)
            {
                log.Error(""This was an error"", ex); 
                // FIXME: I should write a good comment here
            }".AsMethodCode();

            VerifyCSharpDiagnostic(test, new DiagnosticResult
            {
                Id = DiagnosticIds.TodoCommentOnExceptionHandler,
                Message = "ToDo comments should be resolved and this exception should be properly handled",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 14, 13)
                }
            });
        }


        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TodoCommentOnExceptionHandlerAnalyzer();
        }
    }
}
