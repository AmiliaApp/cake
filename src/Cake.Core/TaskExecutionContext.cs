using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.Core
{
    /// <summary>
    /// Acts as a context providing info about a <see cref="CakeTask"/> during its invocation.
    /// </summary>
    public sealed class TaskExecutionContext : CakeContextAdapter, ITaskExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskExecutionContext"/> class.
        /// </summary>
        /// <param name="context">The Cake Context.</param>
        /// <param name="task">The task.</param>
        public TaskExecutionContext(ICakeContext context, ICakeTaskInfo task)
            : base(context)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            Task = task;
        }

        /// <summary>
        /// Gets the <see cref="ICakeTaskInfo"/> describing the <see cref="CakeTask"/> that has just been invoked.
        /// </summary>
        /// <value>
        /// The task.
        /// </value>
        public ICakeTaskInfo Task { get; }
    }
}
