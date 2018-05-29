using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cake.Core.IO;
using Cake.Core.Tooling;

namespace Cake.Common.Tools.CommandPrompt
{
    /// <summary>
    /// Contains settings used by <see cref="CommandPromptRunner"/>.
    /// </summary>
    public sealed class CommandPromptSettings : ToolSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the command prompt terminates or remains after execution of the command
        /// </summary>
        public bool TerminateAfterExecution { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command should strip the first and last quotes
        /// </summary>
        public bool StripFirstAndLastQuotes { get; set; }
    }
}
