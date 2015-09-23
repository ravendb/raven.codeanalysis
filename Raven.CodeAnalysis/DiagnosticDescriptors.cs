using Microsoft.CodeAnalysis;

namespace Raven.CodeAnalysis
{
    public static class DiagnosticDescriptors
    {
		public static readonly DiagnosticDescriptor Logging = new DiagnosticDescriptor(
			id: DiagnosticIds.Logging,
			title: "Wrap Debug and DebugExcepption with IsDebugEnabled condition",
			messageFormat: "Wrap Debug and DebugException with IsDebugEnabled condition",
			category: DiagnosticCategories.Logging,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static readonly DiagnosticDescriptor ConfigureAwait = new DiagnosticDescriptor(
			id: DiagnosticIds.ConfigureAwait,
			title: "Awaited operations must have ConfigureAwait(false)",
			messageFormat: "Awaited operations must have ConfigureAwait(false)",
			category: DiagnosticCategories.ConfigureAwait,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);
	}
}
