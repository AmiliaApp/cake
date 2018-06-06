using System;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;

namespace Cake.Common.Diagnostics
{
    /// <summary>
    /// Contains functionality related to logging including the current task name.
    /// </summary>
    public static class LoggingWithTaskAliases
    {
        /// <summary>
        /// Writes an error message to the log using the specified string value.
        /// </summary>
        /// <param name="context">the context.</param>
        /// <param name="taskName">The task name.</param>
        /// <param name="value">The value.</param>
        /// <example>
        /// <code>
        /// ErrorWithTask("{taskName}", "{string}");
        /// </code>
        /// </example>
        [CakeMethodAlias]
        [CakeAliasCategory("Error")]
        public static void ErrorWithTask(this ICakeContext context, string taskName, string value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Log.Error(PrependTaskName(taskName, value));
        }

        /// <summary>
        /// Writes an warning message to the log using the specified string value.
        /// </summary>
        /// <param name="context">the context.</param>
        /// <param name="taskName">The task name.</param>
        /// <param name="value">The value.</param>
        /// <example>
        /// <code>
        /// WarningWithTask("{taskName}", "{string}");
        /// </code>
        /// </example>
        [CakeMethodAlias]
        [CakeAliasCategory("Warning")]
        public static void WarningWithTask(this ICakeContext context, string taskName, string value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Log.Warning(PrependTaskName(taskName, value));
        }

        /// <summary>
        /// Writes an informational message to the log using the specified string value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="taskName">The task name.</param>
        /// <param name="value">The value.</param>
        /// <example>
        /// <code>
        /// InformationWithTask("{taskName}", "{string}");
        /// </code>
        /// </example>
        [CakeMethodAlias]
        [CakeAliasCategory("Information")]
        public static void InformationWithTask(this ICakeContext context, string taskName, string value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Log.Information(PrependTaskName(taskName, value));
        }

        /// <summary>
        /// Writes an verbose message to the log using the specified string value.
        /// </summary>
        /// <param name="context">the context.</param>
        /// <param name="taskName">The task name.</param>
        /// <param name="value">The value.</param>
        /// <example>
        /// <code>
        /// VerboseWithTask("{taskName}", "{string}");
        /// </code>
        /// </example>
        [CakeMethodAlias]
        [CakeAliasCategory("Verbose")]
        public static void VerboseWithTask(this ICakeContext context, string taskName, string value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Log.Verbose(PrependTaskName(taskName, value));
        }

        /// <summary>
        /// Writes an debug message to the log using the specified string value.
        /// </summary>
        /// <param name="context">the context.</param>
        /// <param name="taskName">The task name.</param>
        /// <param name="value">The value.</param>
        /// <example>
        /// <code>
        /// DebugWithTask("{taskName}", "{string}");
        /// </code>
        /// </example>
        [CakeMethodAlias]
        [CakeAliasCategory("Debug")]
        public static void DebugWithTask(this ICakeContext context, string taskName, string value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Log.Debug(PrependTaskName(taskName, value));
        }

        /// <summary>
        /// Format the given <c>value</c> with the <c>taskName</c> prepended in angle brackets.
        /// </summary>
        /// <param name="taskName">The taskname to prepend</param>
        /// <param name="value">The log to write</param>
        /// <returns>A formatted string with the <c>taskName</c> prepended in angle brackets</returns>
        public static string PrependTaskName(string taskName, string value)
        {
            return $"<{taskName}> {value}";
        }
    }
}
