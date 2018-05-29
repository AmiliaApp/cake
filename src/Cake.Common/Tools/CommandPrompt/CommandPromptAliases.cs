using System;
using Cake.Core;
using Cake.Core.Annotations;

namespace Cake.Common.Tools.CommandPrompt
{
    /// <summary>
    /// <para>Contains functionality related to running a process.</para>
    /// </summary>
    public static class CommandPromptAliases
    {
        /// <summary>
        /// Runs the specified process.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="configurator">The settings configurator.</param>
        [CakeMethodAlias]
        public static void CommandPrompt(this ICakeContext context, Action<CommandPromptSettings> configurator)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            var settings = new CommandPromptSettings();
            configurator(settings);

            // Perform the build.
            CommandPrompt(context, settings);
        }

        /// <summary>
        /// Runs the specified process.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="settings">The settings.</param>
        [CakeMethodAlias]
        public static void CommandPrompt(this ICakeContext context, CommandPromptSettings settings)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var runner = new CommandPromptRunner(context.FileSystem, context.Environment, context.ProcessRunner, context.Tools);
            runner.Run(settings);
        }
    }
}