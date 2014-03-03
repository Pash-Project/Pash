// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.ObjectModel;
using Pash.Implementation;
using System.Reflection;
using System.IO;
using System.Collections;

namespace Pash
{
    internal class FullHost
    {
        private Runspace _currentRunspace;

        public const string BannerText = "Pash - Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/";

        internal LocalHost LocalHost { get; private set; }

        public FullHost()
        {
            LocalHost = new LocalHost();
            _currentRunspace = RunspaceFactory.CreateRunspace(LocalHost);
            _currentRunspace.Open();

            // TODO: check user directory for a config script and fall back to the default one of the installation
            // need to use .CodeBase property, as .Location gets the wrong path if the assembly was shadow-copied
            var localAssemblyPath = new Uri(Assembly.GetCallingAssembly().CodeBase).LocalPath;
            Execute(FormatConfigCommand(localAssemblyPath));
        }

        static internal string FormatConfigCommand(string path)
        {
            return ". \"" + Path.Combine(Path.GetDirectoryName(path), "config.ps1") + "\"";
        }

        void Execute(string cmd)
        {
            var errors = new Collection<object>();
            bool hadSuccess = false;
            try
            {
                // execute the command with no input...
                hadSuccess = executeHelper(cmd, null, ref errors);
            }
            catch (Exception e)
            {
                // An exception occurred that we want to display
                // using the display formatter. To do this we run
                // a second pipeline passing in the error record.
                // The runtime will bind this to the $input variable
                // which is why $input is being piped to out-default
                hadSuccess = false;
                errors.Add(e);
            }
            if (!hadSuccess && errors.Count > 0)
            {
                executeHelper("out-default", new ArrayList(errors).ToArray(), ref errors);
            }
        }


        public int Run()
        {
            return Run(true, null);
        }

        public int Run(bool interactive, string commands)
        {
            /* LocalHostUserInterface supports getline.cs to provide more comfort for non-Windows users.
             * By default, getline.cs is used on non-Windows systems to hanle user input. However, it can be controlled
             * on all systems with the PASHUseUnixLikeConsole variable. As this has nothing to do with the PS
             * specification, this option is specific to LocalHostUserInterface and cannot be set otherwise.
             */
            var ui = LocalHost.UI;
            var localUI = ui as LocalHostUserInterface;
            if (localUI != null)
            {
                bool? useUnixLikeConsole = GetBoolVariable (PashVariables.UseUnixLikeConsole);
                localUI.UseUnixLikeInput = useUnixLikeConsole ?? localUI.UseUnixLikeInput;
            }

            if (String.IsNullOrEmpty(commands))
            { 
                ui.WriteLine(ConsoleColor.White, ConsoleColor.Black, BannerText);
                ui.WriteLine();
            }
            else
            {
                Execute(commands);
            }

            // exit now if we don't want an interactive prompt
            if (!interactive)
            {
                return LocalHost.ExitCode;
            }

            // Loop reading commands to execute until ShouldExit is set by
            // the user calling "exit".
            while (!LocalHost.ShouldExit)
            {
                Prompt();

                string cmd = ui.ReadLine();

                if (cmd == null) // EOF
                    break;

                // TODO: remove the cheat - control via script and ShouldExit
                if (string.Compare(cmd.Trim(), "exit", true) == 0)
                    break;

                Execute(cmd);
            }

            // Exit with the desired exit code that was set by exit command.
            // This is set in the host by the MyHost.SetShouldExit() implementation.
            return LocalHost.ExitCode;
        }

        internal void Prompt()
        {
            Execute("prompt | write-host -nonewline");
        }

        private bool? GetBoolVariable (string name)
        {
            var variable = _currentRunspace.SessionStateProxy.GetVariable(name) as PSVariable;
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

        private bool executeHelper(string cmd, object[] input, ref Collection<object> errors)
        {
            // Ignore empty command lines.
            if (String.IsNullOrEmpty(cmd))
                return true;

            bool success = true;
            using (var currentPipeline = _currentRunspace.CreatePipeline())
            {
                // A command is not a simple word here, it's the whole user input and might contain
                // multiple commands. Therefore we parse it first, but make sure it's not executed in a local scope
                currentPipeline.Commands.AddScript(cmd, false);

                // Now add the default outputter to the end of the pipe.
                // This will result in the output being written using the PSHost
                // and PSHostUserInterface classes instead of returning objects to the hosting
                // application.
                currentPipeline.Commands.Add("out-default");

                // If there was any input specified, pass it in, otherwise just
                // execute the pipeline.
                if (input != null)
                {
                    currentPipeline.Invoke(input);
                }
                else
                {
                    currentPipeline.Invoke();
                }
                // if the pipeline failed, not everything was printed by the out-default command. print the errors
                if (currentPipeline.PipelineStateInfo.State.Equals(PipelineState.Failed))
                {
                    if (errors != null)
                    {
                        errors = currentPipeline.Error.ReadToEnd();
                    }
                    success = false;
                }
            }
            return success;
        }
    }
}
