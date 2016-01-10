
namespace Raven.CodeAnalysis.Test
{
    static class CompleteCodeExtensions
    {
        public static string AsMethodCode(this string code)
        {
            return @"
using System;
namespace FooNamespace
{
    class FooClass
    {
        public void FooMethod()
        {
" + code + @"
        }
    }
}";
        }
    }
}
