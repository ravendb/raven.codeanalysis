namespace Raven.CodeAnalysis
{
    internal static class DiagnosticIds
    {
	    public const string Logging = "RDB0001";

		public const string ConfigureAwait = "RDB0002";

        public const string GetConfigurationEntryKey = "RDB0003";

        public const string EmptyOrJustLoggingExceptionHandler = "RDB0004";

        public const string TodoCommentOnExceptionHandler = "RDB0005";

        public const string ValueTupleVariablesMustBeUppercase = "RDB0006";

        public const string CancellationTokenMustBeLastArgument = "RDB0007";

        public const string TaskCompletionSourceMustHaveRunContinuationsAsynchronouslySet = "RDB0008";
    }
}
