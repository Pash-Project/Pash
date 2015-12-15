// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
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

        public override void Prepare()
        {
            // nothing to do, applcation is completely executed in the ProcessRecords phas
        }

        public override void BeginProcessing()
        {
            // nothing to do
        }

        public override void ProcessRecords()
        {
            // TODO: make a check if process is already started. ProcessRecords() can be called multiple times
            var flag = GetPSForceSynchronizeProcessOutput();
            _shouldBlock = NeedWaitForProcess(flag, ApplicationInfo.Path);
            _process = StartProcess();

            foreach (var curInput in CommandRuntime.InputStream.Read())
            {
                if (curInput != null)
                {
                    _process.StandardInput.WriteLine(curInput.ToString());
                }
            }

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

        public override void EndProcessing()
        {
            // As the process can run asynchronous, cleanup is handled by ProcessExited
        }

        private ApplicationInfo ApplicationInfo
        {
            get
            {
                return (ApplicationInfo)CommandInfo;
            }
        }

        private void ProcessExited(object sender, System.EventArgs e)
        {
            ExecutionContext.SetLastExitCodeVariable(_process.ExitCode);
            ExecutionContext.SetSuccessVariable(_process.ExitCode == 0); // PS also just compares against null

            _process.Dispose();
            _process = null;
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
            process.Exited += new EventHandler(ProcessExited);

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
                // also: in parameter.Name are arguments that started with "-", so we need to use them again
                if (!String.IsNullOrEmpty(parameter.Name))
                {
                    arguments.Append('-');
                    arguments.Append(parameter.Name); // parameter names don't hava a whitespace
                    arguments.Append(' ');
                }

                if (parameter.Value != null)
                {
                    object pValue = parameter.Value;

                    if (pValue is PSObject)
                    {
                        pValue = ((PSObject)pValue).BaseObject;
                    }

                    IEnumerable values;

                    if (pValue is IEnumerable &&
                        !(pValue is string))
                    {
                        values = (IEnumerable)pValue;
                    }
                    else
                    {
                        values = new object[] { pValue };
                    }

                    foreach (var value in values)
                    {
                        var argument = value.ToString();
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
                }
            }

            return arguments.ToString();
        }
    }
}

