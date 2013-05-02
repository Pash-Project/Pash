// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;

namespace Pash.Implementation
{
    /// <summary>
    /// Command processor for the application command. This is command for executing external file.
    /// </summary>
    internal class ApplicationProcessor : CommandProcessorBase
    {
        private Process _process;

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
            // TODO: If command was called not as part of another expression then we don't need to redirect input and output.
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
            // Release GUI programs immediately.
            if (ProcessHasGui())
            {
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
                RedirectStandardOutput = true
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
        /// Checks if process represents a graphical application.
        /// </summary>
        /// <returns><c>true</c>, if a process represents a graphical application, <c>false</c> otherwise.</returns>
        private bool ProcessHasGui()
        {
            bool hasGui;
            try
            {
                hasGui = _process.WaitForInputIdle();
            }
            catch (InvalidOperationException)
            {
                // WaitForInputIdle throws this exception if a process haven't GUI.
                hasGui = false;
            }

            return hasGui;
        }
    }
}

