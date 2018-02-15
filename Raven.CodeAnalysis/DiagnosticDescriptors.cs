﻿using Microsoft.CodeAnalysis;

namespace Raven.CodeAnalysis
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor ValueTuple = new DiagnosticDescriptor(
            id: DiagnosticIds.ValueTupleVariablesMustBeUppercase,
            title: "Use PascalCase in named ValueTuples",
            messageFormat: "Use PascalCase in named ValueTuples",
            category: DiagnosticCategories.ValueTuple,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor Logging = new DiagnosticDescriptor(
            id: DiagnosticIds.Logging,
            title: "Wrap Debug and DebugException with IsDebugEnabled condition",
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

        public static readonly DiagnosticDescriptor GetConfigurationEntryKey = new DiagnosticDescriptor(
            id: DiagnosticIds.GetConfigurationEntryKey,
            title: "Specified property is not a configuration entry",
            messageFormat: "'{0}' property is not decorated with [ConfigurationEntry] attribute",
            category: DiagnosticCategories.Configuration,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        public static readonly DiagnosticDescriptor EmptyOrJustLoggingExceptionHandler = new DiagnosticDescriptor(
            id: DiagnosticIds.EmptyOrJustLoggingExceptionHandler,
            title: "Exception handler is empty or just logging",
            messageFormat: "This exception should be properly handled",
            category: DiagnosticCategories.ExceptionBlock,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor TodoCommentOnExceptionHandler = new DiagnosticDescriptor(
            id: DiagnosticIds.TodoCommentOnExceptionHandler,
            title: "ToDo Comments on Exception Handler",
            messageFormat: "ToDo comments should be resolved and this exception should be properly handled",
            category: DiagnosticCategories.ExceptionBlock,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor CancellationTokenMustBeLastArgument = new DiagnosticDescriptor(
            id: DiagnosticIds.CancellationTokenMustBeLastArgument,
            title: "CancellationToken must be a last argument",
            messageFormat: "CancellationToken must be a last argument",
            category: DiagnosticCategories.CancellationToken,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor TaskCompletionSourceMustHaveRunContinuationsAsynchronouslySet = new DiagnosticDescriptor(
            id: DiagnosticIds.TaskCompletionSourceMustHaveRunContinuationsAsynchronouslySet,
            title: "TaskCompletionSource must have TaskCreationOptions.RunContinuationsAsynchronously set",
            messageFormat: "TaskCompletionSource must have TaskCreationOptions.RunContinuationsAsynchronously set",
            category: DiagnosticCategories.TaskCompletionSource,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MustNotReturnTaskInsideUsingStatementAnalyzer = new DiagnosticDescriptor(
            id: DiagnosticIds.MustNotReturnTaskInsideUsingStatementAnalyzer,
            title: "Cannot return task without awaiting it inside using statement",
            messageFormat: "Cannot return task without awaiting it inside using statement",
            category: DiagnosticCategories.ReturningTaskInsideUsingStatement,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
