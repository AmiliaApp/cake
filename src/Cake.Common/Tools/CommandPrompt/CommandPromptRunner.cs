using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cake.Core;
using Cake.Core.IO;
using Cake.Core.Tooling;

namespace Cake.Common.Tools.CommandPrompt
{
    /// <summary>
    /// The process runner.
    /// </summary>
    public sealed class CommandPromptRunner : Tool<CommandPromptSettings>
    {
        private readonly ICakeEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandPromptRunner"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="runner">The runner.</param>
        /// <param name="tools">The tool locator.</param>
        public CommandPromptRunner(
            IFileSystem fileSystem,
            ICakeEnvironment environment,
            IProcessRunner runner,
            IToolLocator tools) : base(fileSystem, environment, runner, tools)
        {
            _environment = environment;
        }

        /// <summary>
        /// Runs a process with the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void Run(CommandPromptSettings settings)
        {
            RunCommand(settings, GetArguments(settings));
        }

        private void RunCommand(CommandPromptSettings settings, ProcessArgumentBuilder arguments)
        {
            var toolPath = new FilePath("cmd");

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            // Should we customize the arguments?
            if (settings.ArgumentCustomization != null)
            {
                arguments = settings.ArgumentCustomization(arguments);
            }

            var process = RunProcess(settings, toolPath, arguments, null);

            WaitForProcess(process, settings, null);
        }

        private ProcessArgumentBuilder GetArguments(CommandPromptSettings settings)
        {
            var builder = new ProcessArgumentBuilder();

            if (settings.StripFirstAndLastQuotes)
            {
                builder.Append("/S");
            }

            if (settings.TerminateAfterExecution)
            {
                builder.Append("/C");
            }
            else
            {
                builder.Append("/K");
            }

            return builder;
        }

        /// <summary>
        /// Gets the name of the tool.
        /// </summary>
        /// <returns>The name of the tool.</returns>
        protected override string GetToolName()
        {
            return "CommandPrompt";
        }

        /// <summary>
        /// Gets the possible names of the tool executable.
        /// </summary>
        /// <returns>The tool executable name.</returns>
        protected override IEnumerable<string> GetToolExecutableNames()
        {
            return Enumerable.Empty<string>();
        }
    }
}
