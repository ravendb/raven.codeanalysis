using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.CodeAnalysis.ReturnNotAwaitedTaskInsideUsing;
using TestHelper;

namespace Raven.CodeAnalysis.Test
{
    [TestClass]
    public class ReturningNotAwaitedTaskInsideUsing: CodeFixVerifier
    {
        [TestMethod]
        public void TestMethod1()
        {
            const string input = @"
class C
{
    public Task<object> Foo()
    {
        using (var item = new Item())
        {
            return Task.Run(() => new object());
        }
    }

    public Task Bar()
    {
        using (var item = new Item())
        {
            return Task.Run(() => new object());
        }
    }

    public async Task BarOk()
    {
        using (var item = new Item())
        {
            await Task.Run(() => new object());
        }
    }

    public async Task<object> FooOk()
    {
        using (var item = new Item())
        {
            return await Task.Run(() => new object());
        }
    }

    public int Faz()
    {
        using (var item = new Item())
        {
            return 1;
        }
    }

    public void Far()
    {
        using (var item = new Item())
        {
                
        }
    }

    public Task Fag()
    {
        using (var item = new Item())
        {
            return Task.CompletedTask;
        }
    }

    public async Task<int> Fah()
    {
        using (var item = new Item())
        {
            await Task.CompletedTask;
                
            return 1;
        }
    }

    public async Task<int> Faj()
    {
        await Task.CompletedTask;

        using (var item = new Item())
        {  
            return 1;
        }
    }

    public Task<int> Fal()
    {
        using (var item = new Item())
        {  
            return Task.FromResult(1);
        }
    }

    public void A()
    {
    }
}";
            
            VerifyCSharpDiagnostic(
                input,
                new DiagnosticResult
                {
                    Id = DiagnosticIds.MustNotReturnTaskInsideUsingStatementAnalyzer,
                    Message = "Cannot return task without awaiting it inside using statement",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 8, 13)
                    }
                },
                new DiagnosticResult
                {
                    Id = DiagnosticIds.MustNotReturnTaskInsideUsingStatementAnalyzer,
                    Message = "Cannot return task without awaiting it inside using statement",
                    Severity = DiagnosticSeverity.Error,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 16, 13)
                    }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MustNotReturnTaskInsideUsingStatementAnalyzer();
        }
    }
}