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
        /// <summary>
        /// Initializes a new instance of the <see cref="Pash.Implementation.ApplicationProcessor"/> class.
        /// </summary>
        /// <param name="commandInfo">Command info.</param>
        public ApplicationProcessor(ApplicationInfo commandInfo)
            : base(commandInfo)
        {
        }

        internal override void BindArguments(PSObject obj)
        {
        }

        internal override void Initialize()
        {
        }

        internal override void ProcessRecord()
        {
            // TODO: If command was called not as part of another expression then we don't need to redirect input and output.
            var process = StartProcess();
            var output = process.StandardOutput;

            // TODO: If present, pass one object to standard input.

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
            // TODO: Dispose process maybe?
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
                arguments.Append(parameter.Value);
                arguments.Append(' ');
            }

            return arguments.ToString();
        }
    }
}

