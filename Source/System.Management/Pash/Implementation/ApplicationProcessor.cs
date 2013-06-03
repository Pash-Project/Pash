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
    public class ApplicationProcessor : CommandProcessorBase
    {
        private Process _process;
        private bool _shouldBlock;

        public ApplicationProcessor(ApplicationInfo commandInfo)
            : base(commandInfo)
        {
        }

        public static bool NeedWaitForProcess(bool? forceSynchronize, string executablePath)
        {
            return (forceSynchronize ?? false) || IsConsoleSubsystem(executablePath);
        }

        public static bool IsConsoleSubsystem(string executablePath)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                // Under UNIX all applications are effectively console.
                return true;
            }

            var info = new Shell32.SHFILEINFO();
            var executableType = (uint)Shell32.SHGetFileInfo(executablePath, 0u, ref info, (uint)Marshal.SizeOf(info), Shell32.SHGFI_EXETYPE);
            return executableType == Shell32.MZ || executableType == Shell32.PE;
        }

        internal override void Initialize()
        {
            var flag = GetPSForceSynchronizeProcessOutput();
            _shouldBlock = NeedWaitForProcess(flag, ApplicationInfo.Path);
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

        private bool? GetPSForceSynchronizeProcessOutput()
        {
            var variable = ExecutionContext.GetVariable(PashVariables.ForceSynchronizeProcessOutput) as PSVariable;
            if (variable == null)
            {
                return null;
            }

            var value = variable.Value;
            var psObject = value as PSObject;
            if (psObject == null)
            {
                return value as bool?;
            }
            else
            {
                return psObject.BaseObject as bool?;
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
    }
}

