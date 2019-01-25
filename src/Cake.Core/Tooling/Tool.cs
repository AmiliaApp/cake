// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Cake.Core.IO;

namespace Cake.Core.Tooling
{
    /// <summary>
    /// Base class for tools.
    /// </summary>
    /// <typeparam name="TSettings">The settings type.</typeparam>
    public abstract class Tool<TSettings> where TSettings : ToolSettings
    {
        private readonly ICakeEnvironment _environment;
        private readonly IFileSystem _fileSystem;
        private readonly IGlobber _globber;
        private readonly IToolLocator _tools;
        private readonly IProcessRunner _processRunner;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tool{TSettings}" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="processRunner">The process runner.</param>
        /// <param name="globber">The globber.</param>
        [Obsolete("Please use Tool(IFileSystem, ICakeEnvironment, IProcessRunner, IToolLocator) instead.")]
        protected Tool(IFileSystem fileSystem, ICakeEnvironment environment, IProcessRunner processRunner, IGlobber globber)
            : this(fileSystem, environment, processRunner, (IToolLocator)null)
        {
            if (globber == null)
            {
                throw new ArgumentNullException(nameof(globber));
            }

            _globber = globber;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tool{TSettings}"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="processRunner">The process runner.</param>
        /// <param name="tools">The tool locator.</param>
        protected Tool(IFileSystem fileSystem, ICakeEnvironment environment, IProcessRunner processRunner, IToolLocator tools)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            if (processRunner == null)
            {
                throw new ArgumentNullException(nameof(processRunner));
            }

            _fileSystem = fileSystem;
            _environment = environment;
            _processRunner = processRunner;
            _tools = tools;
        }

        /// <summary>
        /// Runs the tool using the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="arguments">The arguments.</param>
        protected void Run(TSettings settings, ProcessArgumentBuilder arguments)
        {
            Run(settings, arguments, null, null);
        }

        private async Task RedirectedProcessOutput(IProcess process, Action<string> standardOutputAction, Action<string> errorOutputAction)
        {
            var standardOutputTask = Task.CompletedTask;

            if (standardOutputAction != null)
            {
                standardOutputTask = Task.Run(() =>
                {
                    foreach (var standardOutput in process.GetStandardOutput())
                    {
                        standardOutputAction(standardOutput);
                    }
                });
            }

            var errorOutputTask = Task.CompletedTask;
            if (errorOutputAction != null)
            {
                standardOutputTask = Task.Run(() =>
                {
                    foreach (var errorOutput in process.GetStandardError())
                    {
                        errorOutputAction(errorOutput);
                    }
                });
            }

            await Task.WhenAll(standardOutputTask, errorOutputTask).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs the tool using a custom tool path and the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="processSettings">The process settings</param>
        /// <param name="postAction">If specified called after process exit</param>
        protected void Run(
            TSettings settings,
            ProcessArgumentBuilder arguments,
            ProcessSettings processSettings,
            Action<IProcess> postAction)
        {
            if (arguments == null && (processSettings?.Arguments == null))
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            var process = RunProcess(settings, arguments, processSettings);

            WaitForProcess(process, settings, postAction);
        }

        /// <summary>
        /// Redirects the output if it is configured to and waits for the given process to complete.
        /// </summary>
        /// <param name="process">The process to wait for</param>
        /// <param name="settings">The settings.</param>
        /// <param name="postAction">If specified called after process exit</param>
        protected void WaitForProcess(
            IProcess process,
            TSettings settings,
            Action<IProcess> postAction)
        {
            var outputRedirectTask = RedirectedProcessOutput(process, settings.RedirectStandardOutputAction, settings.RedirectStandardErrorAction);

            // Wait for the process to exit.
            if (settings.ToolTimeout.HasValue)
            {
                if (!process.WaitForExit((int)settings.ToolTimeout.Value.TotalMilliseconds))
                {
                    const string message = "Tool timeout ({0}): {1}";
                    throw new TimeoutException(string.Format(CultureInfo.InvariantCulture, message, settings.ToolTimeout.Value, GetToolName()));
                }
            }
            else
            {
                process.WaitForExit();
                outputRedirectTask.GetAwaiter().GetResult();
            }

            try
            {
                ProcessExitCode(process.GetExitCode());
            }
            finally
            {
                // Post action specified?
                postAction?.Invoke(process);
            }
        }

        /// <summary>
        /// Customized exit code handling.
        /// Standard behavior is to fail when non zero.
        /// </summary>
        /// <param name="exitCode">The process exit code</param>
        protected virtual void ProcessExitCode(int exitCode)
        {
            // Did an error occur?
            if (exitCode != 0)
            {
                const string message = "{0}: Process returned an error (exit code {1}).";
                throw new CakeException(string.Format(CultureInfo.InvariantCulture, message, GetToolName(), exitCode));
            }
        }

        /// <summary>
        /// Runs the tool using a custom tool path and the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The process that the tool is running under.</returns>
        protected IProcess RunProcess(TSettings settings, ProcessArgumentBuilder arguments)
        {
            return RunProcess(settings, arguments, null);
        }

        /// <summary>
        /// Runs the tool using a custom tool path and the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="processSettings">The process settings</param>
        /// <returns>The process that the tool is running under.</returns>
        protected IProcess RunProcess(
            TSettings settings,
            ProcessArgumentBuilder arguments,
            ProcessSettings processSettings)
        {
            if (arguments == null && (processSettings?.Arguments == null))
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            // Should we customize the arguments?
            if (settings.ArgumentCustomization != null)
            {
                arguments = settings.ArgumentCustomization(arguments);
            }

            // Get the tool path.
            var toolPath = GetToolPath(settings);
            if (toolPath == null || !_fileSystem.Exist(toolPath))
            {
                // Get the tool name.
                var toolName = GetToolName();

                const string message = "{0}: Could not locate executable.";
                throw new CakeException(string.Format(CultureInfo.InvariantCulture, message, toolName));
            }

            return RunProcess(settings, toolPath, arguments, processSettings);
        }

        /// <summary>
        /// Runs the tool using the given relative or absolute toolPath which specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="toolPath">The relativeo or absolute tool path.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="processSettings">The process settings.</param>
        /// <returns>The process that the tool is running under.</returns>
        protected IProcess RunProcess(
            TSettings settings,
            FilePath toolPath,
            ProcessArgumentBuilder arguments,
            ProcessSettings processSettings)
        {
            // Get the tool name.
            var toolName = GetToolName();

            // Get the working directory.
            var workingDirectory = GetWorkingDirectory(settings);
            if (workingDirectory == null)
            {
                const string message = "{0}: Could not resolve working directory.";
                throw new CakeException(string.Format(CultureInfo.InvariantCulture, message, toolName));
            }

            // Create the process start info.
            var info = processSettings ?? new ProcessSettings();
            if (info.Arguments == null)
            {
                info.Arguments = arguments;
            }
            if (info.WorkingDirectory == null)
            {
                info.WorkingDirectory = workingDirectory.MakeAbsolute(_environment).FullPath;
            }
            if (info.EnvironmentVariables == null)
            {
                info.EnvironmentVariables = GetEnvironmentVariables(settings);
            }

            if (settings.RedirectStandardErrorAction != null)
            {
                info.RedirectStandardError = true;
            }

            if (settings.RedirectStandardOutputAction != null)
            {
                info.RedirectStandardOutput = true;
            }

            // Run the process.
            var process = _processRunner.Start(toolPath, info);
            if (process == null)
            {
                const string message = "{0}: Process was not started.";
                throw new CakeException(string.Format(CultureInfo.InvariantCulture, message, toolName));
            }
            return process;
        }

        /// <summary>
        /// Gets the name of the tool.
        /// </summary>
        /// <returns>The name of the tool.</returns>
        protected abstract string GetToolName();

        /// <summary>
        /// Gets the possible names of the tool executable.
        /// </summary>
        /// <returns>The tool executable name.</returns>
        protected abstract IEnumerable<string> GetToolExecutableNames();

        /// <summary>
        /// Gets the working directory.
        /// Defaults to the currently set working directory.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The working directory for the tool.</returns>
        protected virtual DirectoryPath GetWorkingDirectory(TSettings settings)
        {
            if (settings.WorkingDirectory != null)
            {
                return settings.WorkingDirectory;
            }

            return _environment.WorkingDirectory;
        }

        /// <summary>
        /// Gets alternative file paths which the tool may exist in
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The default tool path.</returns>
        protected virtual IEnumerable<FilePath> GetAlternativeToolPaths(TSettings settings)
        {
            return Enumerable.Empty<FilePath>();
        }

        /// <summary>
        /// Gets the environment variables.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The environment variables for the tool.</returns>
        protected virtual IDictionary<string, string> GetEnvironmentVariables(TSettings settings)
        {
            return settings.EnvironmentVariables;
        }

        /// <summary>
        /// Gets the resolved tool path.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The resolved tool path.</returns>
        protected FilePath GetToolPath(TSettings settings)
        {
            if (_tools != null)
            {
                return GetToolPathUsingToolService(settings);
            }

            return GetToolPathObsolete(settings);
        }

        private FilePath GetToolPathUsingToolService(TSettings settings)
        {
            var toolPath = settings.ToolPath;
            if (toolPath != null)
            {
                return toolPath.MakeAbsolute(_environment);
            }

            // Look for each possible executable name in various places.
            var toolExeNames = GetToolExecutableNames();
            foreach (var toolExeName in toolExeNames)
            {
                var result = _tools.Resolve(toolExeName);
                if (result != null)
                {
                    return result;
                }
            }

            // Look through all the alternative directories for the tool.
            var alternativePaths = GetAlternativeToolPaths(settings) ?? Enumerable.Empty<FilePath>();
            foreach (var alternativePath in alternativePaths)
            {
                if (_fileSystem.Exist(alternativePath))
                {
                    return alternativePath.MakeAbsolute(_environment);
                }
            }

            return null;
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
        private FilePath GetToolPathObsolete(TSettings settings)
        {
            var toolPath = settings.ToolPath;
            if (toolPath != null)
            {
                return toolPath.MakeAbsolute(_environment);
            }

            var toolExeNames = GetToolExecutableNames();
            string[] pathDirs = null;

            // Look for each possible executable name in various places.
            foreach (var toolExeName in toolExeNames)
            {
                // First look in ./tools/
                toolPath = _globber.GetFiles("./tools/**/" + toolExeName).FirstOrDefault();
                if (toolPath != null)
                {
                    return toolPath.MakeAbsolute(_environment);
                }

                // Cache the PATH directory list if we didn't already.
                if (pathDirs == null)
                {
                    var pathEnv = _environment.GetEnvironmentVariable("PATH");
                    if (!string.IsNullOrEmpty(pathEnv))
                    {
                        pathDirs = pathEnv.Split(new[] { _environment.Platform.IsUnix() ? ':' : ';' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        pathDirs = new string[] { };
                    }
                }

                // Look in every PATH directory for the file.
                foreach (var pathDir in pathDirs)
                {
                    var file = new DirectoryPath(pathDir).CombineWithFilePath(toolExeName);
                    if (_fileSystem.Exist(file))
                    {
                        return file.MakeAbsolute(_environment);
                    }
                }
            }

            // Look through all the alternative directories for the tool.
            var alternativePaths = GetAlternativeToolPaths(settings) ?? Enumerable.Empty<FilePath>();
            foreach (var altPath in alternativePaths)
            {
                if (_fileSystem.Exist(altPath))
                {
                    return altPath.MakeAbsolute(_environment);
                }
            }

            return null;
        }
    }
}