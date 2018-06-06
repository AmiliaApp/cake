using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.Diagnostics;
using NSubstitute;
using Xunit;

namespace Cake.Common.Tests.Unit.Diagnostics
{
    public class LoggingWithTaskAliasesTests
    {
        public sealed class TheErrorPrependTaskName
        {
            [Fact]
            public void Should_Prepend_Task_Name_In_Angle_Brackets()
            {
                // Given
                var taskName = "taskName";
                var value = "thing to log";

                // When
                var prependedWithTaskName = LoggingWithTaskAliases.PrependTaskName(taskName, value);

                // Then
                Assert.Equal($"<{taskName}> {value}", prependedWithTaskName);
            }
        }

        public sealed class TheErrorMethod
        {
            [Fact]
            public void Should_Write_Error_String_Value_To_Log()
            {
                // Given
                var context = Substitute.For<ICakeContext>();
                context.Log.Returns(Substitute.For<ICakeLog>());

                var taskName = "TestTask";
                const string value = "Hello {0}";
                var prependedWithTaskName = LoggingWithTaskAliases.PrependTaskName(taskName, value);

                // When
                context.ErrorWithTask(taskName, value);

                // Then
                context.Log.Received(1).Write(Verbosity.Quiet, LogLevel.Error, "{0}", prependedWithTaskName);
            }
        }

        public sealed class TheWarningMethod
        {
            [Fact]
            public void Should_Write_Warning_String_Value_To_Log()
            {
                // Given
                var context = Substitute.For<ICakeContext>();
                context.Log.Returns(Substitute.For<ICakeLog>());

                var taskName = "TestTask";
                const string value = "Hello {0}";
                var prependedWithTaskName = LoggingWithTaskAliases.PrependTaskName(taskName, value);

                // When
                context.WarningWithTask(taskName, value);

                // Then
                context.Log.Received(1).Write(Verbosity.Minimal, LogLevel.Warning, "{0}", prependedWithTaskName);
            }
        }

        public sealed class TheInformationMethod
        {
            [Fact]
            public void Should_Write_Information_String_Value_To_Log()
            {
                // Given
                var context = Substitute.For<ICakeContext>();
                context.Log.Returns(Substitute.For<ICakeLog>());

                var taskName = "TestTask";
                const string value = "Hello {0}";
                var prependedWithTaskName = LoggingWithTaskAliases.PrependTaskName(taskName, value);

                // When
                context.InformationWithTask(taskName, value);

                // Then
                context.Log.Received(1).Write(Verbosity.Normal, LogLevel.Information, "{0}", prependedWithTaskName);
            }
        }

        public sealed class TheVerboseMethod
        {
            [Fact]
            public void Should_Write_Verbose_String_Value_To_Log()
            {
                // Given
                var context = Substitute.For<ICakeContext>();
                context.Log.Returns(Substitute.For<ICakeLog>());

                var taskName = "TestTask";
                const string value = "Hello {0}";
                var prependedWithTaskName = LoggingWithTaskAliases.PrependTaskName(taskName, value);

                // When
                context.VerboseWithTask(taskName, value);

                // Then
                context.Log.Received(1).Write(Verbosity.Verbose, LogLevel.Verbose, "{0}", prependedWithTaskName);
            }
        }

        public sealed class TheDebugMethod
        {
            [Fact]
            public void Should_Write_Debug_String_Value_To_Log()
            {
                // Given
                var context = Substitute.For<ICakeContext>();
                context.Log.Returns(Substitute.For<ICakeLog>());

                var taskName = "TestTask";
                const string value = "Hello {0}";
                var prependedWithTaskName = LoggingWithTaskAliases.PrependTaskName(taskName, value);

                // When
                context.DebugWithTask(taskName, value);

                // Then
                context.Log.Received(1).Write(Verbosity.Diagnostic, LogLevel.Debug, "{0}", prependedWithTaskName);
            }
        }
    }
}
