using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.CodeAnalysis.Configuration;
using TestHelper;

namespace Raven.CodeAnalysis.Test
{
    [TestClass]
    public class GetConfigurationEntryKeyTests : CodeFixVerifier
    {
        private const string AttributeCode = @"
namespace Raven.Database.Config.Attributes
{
    using System;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ConfigurationEntryAttribute : Attribute
    {
        public ConfigurationEntryAttribute(string key, [CallerLineNumber]int order = 0)
        {
        }

        public string Key { get; set; }
    }
}";

        private const string RavenConfigurationClassCode = @"
namespace Raven.Database.Config
{
    using System;
    using System.Reflection;
    using System.Linq.Expressions;
    using Raven.Database.Config.Attributes;
    
    public class RavenConfiguration
    {
        [ConfigurationEntry(""Raven/Configuration/Value"")]
        public string ConfigurationValueProperty { get; set; }

        public string OrdinaryProperty { get; set; }

        public static string GetKey(Expression<Func<RavenConfiguration, object>> getKey)
        {
            return null;
        }
    }
}";
        [TestMethod]
        public void ErrorIfSpecifiedPropertyIsNotDecoratedWithConfigurationEntryAttribute()
        {
            const string input = AttributeCode + RavenConfigurationClassCode + @"
class Foo
{
    void Bar()
    {
        Raven.Database.Config.RavenConfiguration.GetKey(x => x.OrdinaryProperty);
    }
}
";
            VerifyCSharpDiagnostic(input, new DiagnosticResult
            {
                Id = DiagnosticIds.GetConfigurationEntryKey,
                Message = "'OrdinaryProperty' property is not decorated with [ConfigurationEntry] attribute",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 41, 64)
                }
            });
        }

        [TestMethod]
        public void ErrorIfSpecifiedPropertyIsNotDecoratedWithConfigurationEntryAttribute_UsageInsideRavenConfiguration()
        {
            const string input = AttributeCode + @"
namespace Raven.Database.Config
{
    using System;
    using System.Reflection;
    using System.Linq.Expressions;
    using Raven.Database.Config.Attributes;
    
    public class RavenConfiguration
    {
        [ConfigurationEntry(""Raven/Configuration/Value"")]
        public string ConfigurationValueProperty { get; set; }

        public string OrdinaryProperty { get; set; }

        public static string GetKey(Expression<Func<RavenConfiguration, object>> getKey)
        {
            return null;
        }

        public void Foo()
        {
            var a = GetKey(x => x.OrdinaryProperty);
        }
    }
}";

            VerifyCSharpDiagnostic(input, new DiagnosticResult
            {
                Id = DiagnosticIds.GetConfigurationEntryKey,
                Message = "'OrdinaryProperty' property is not decorated with [ConfigurationEntry] attribute",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 38, 35)
                }
            });
        }

        [TestMethod]
        public void ErrorIfSpecifiedPropertyIsNotDecoratedWithConfigurationEntryAttribute_UsageInsideIfStatement()
        {
            const string input = AttributeCode + RavenConfigurationClassCode + @"
class Foo
{
    void Bar()
    {
        if (Raven.Database.Config.RavenConfiguration.GetKey(x => x.OrdinaryProperty) == null)
        {
        }
    }
}
";

            VerifyCSharpDiagnostic(input, new DiagnosticResult
            {
                Id = DiagnosticIds.GetConfigurationEntryKey,
                Message = "'OrdinaryProperty' property is not decorated with [ConfigurationEntry] attribute",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 41, 68)
                }
            });
        }

        [TestMethod]
        public void DoNotErrorIfSpecifiedPropertyIsDecoratedWithConfigurationEntryAttribute()
        {
            const string input = AttributeCode + RavenConfigurationClassCode + @"
class Foo
{
    void Bar()
    {
        Raven.Database.Config.RavenConfiguration.GetKey(x => x.ConfigurationValueProperty);
    }
}
";
            VerifyCSharpDiagnostic(input);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new GetConfigurationEntryKeyAnalyzer();
        }
    }
}