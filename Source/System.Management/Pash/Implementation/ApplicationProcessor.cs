// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using Pash.Implementation.Native;

namespace Pash.Implementation
{
    /// <summary>
    /// Command processor for the application command. This is command for executing external file.
    /// </summary>
    internal class ApplicationProcessor : CommandProcessorBase
    {
        private Process _process;
        private bool _shouldBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pash.Implementation.ApplicationProcessor"/> class.
        /// </summary>
        /// <param name="commandInfo">Command info.</param>
        public ApplicationProcessor(ApplicationInfo commandInfo)
            : base(commandInfo)
        {
        }

        internal override void Initialize()
        {
            _shouldBlock = ShouldBlock();
            _process = StartProcess();
        }

        internal override void BindArguments(PSObject obj)
        {
            if (obj != null)
            {
                var inputObject = obj.ToString();
                _process.StandardInput.WriteLine(inputObject);
            }
        }

        internal override void ProcessRecord()
        {
            if (!_shouldBlock)
            {
                return;
            }

            if (!ExecutionContext.WriteSideEffectsToPipeline)
            {
                // TODO: Ctrl-C cancellation?
                _process.WaitForExit();
                return;
            }

            var output = _process.StandardOutput;
            while (!output.EndOfStream)
            {
                var line = output.ReadLine();
                CommandRuntime.WriteObject(line);
            }
        }

        internal override void Complete()
        {
            // TODO: Should we set $LASTEXITCODE here?
            // TODO: Same for the $? variable.
            if (_process != null)
            {
                _process.Dispose();
            }
        }

        internal override ICommandRuntime CommandRuntime
        {
            get;
            set;
        }

        private ApplicationInfo ApplicationInfo
        {
            get
            {
                return (ApplicationInfo)CommandInfo;
            }
        }

        private Process StartProcess()
        {
            var startInfo = new ProcessStartInfo(ApplicationInfo.Path)
            {
                Arguments = PrepareArguments(),
                UseShellExecute = false,
                RedirectStandardOutput = ExecutionContext.WriteSideEffectsToPipeline
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            if (!process.Start())
            {
                throw new Exception("Cannot start process");
            }
            
            return process;
        }

        private string PrepareArguments()
        {
            var arguments = new StringBuilder();
            foreach (var parameter in Parameters)
            {
                // PowerShell quotes any arguments that contain spaces except the arguments that start with a quote.
                var argument = parameter.Value.ToString();
                if (argument.Contains(" ") && !argument.StartsWith("\""))
                {
                    arguments.AppendFormat("\"{0}\"", argument);
                }
                else
                {
                    arguments.Append(argument);
                }

                arguments.Append(' ');
            }

            return arguments.ToString();
        }

        /// <summary>
        /// Checks if thread should be blocked by the process execution.
        /// </summary>
        /// <returns><c>true</c>, if current thread should be blocked, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// In Windows environment, only application with console subsystem should block execution. In Unix, any
        /// application is effectively "console", so any application blocks.
        /// </remarks>
        private bool ShouldBlock()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT && IsConsoleSubsystem(ApplicationInfo.Path);
        }

        /// <summary>
        /// Checks if the Windows executable is a console application.
        /// </summary>
        /// <param name="path">Full path to file.</param>
        /// <returns><c>true</c> if application uses console subsystem.</returns>
        private static bool IsConsoleSubsystem(string path)
        {
            const int MZ = 'M' << 8 + 'Z';
            const int PE = 'P' << 8 + 'E';

            var info = new Shell32.SHFILEINFO();
            switch ((uint)Shell32.SHGetFileInfo(path, 0u, ref info, (uint)Marshal.SizeOf(info), Shell32.SHGFI_EXETYPE))
            {
                case MZ:
                case PE:
                    return true;
                default:
                    return false;
            }
        }
    }
}

