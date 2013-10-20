// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.ObjectModel;
using Pash.Implementation;
using System.Reflection;
using System.IO;
using Mono.Terminal;

namespace Pash
{
    internal class FullHost
    {
        private Runspace myRunSpace;
        private LocalHost myHost;
        private Pipeline currentPipeline;

        public FullHost()
        {
            myHost = new LocalHost();
            myRunSpace = RunspaceFactory.CreateRunspace(myHost);
            myRunSpace.Open();

            string exePath = Assembly.GetCallingAssembly().Location;
            string configPath = Path.Combine(Path.GetDirectoryName(exePath), "config.ps1");

            Execute(configPath);
        }

        void executeHelper(string cmd, object input)
        {
            // Ignore empty command lines.
            if (String.IsNullOrEmpty(cmd))
                return;

            // Create the pipeline object and make it available
            // to the ctrl-C handle through the currentPipeline instance
            // variable.
            currentPipeline = myRunSpace.CreatePipeline();

            // Create a pipeline for this execution. Place the result in the currentPipeline
            // instance variable so that it is available to be stopped.
            try
            {
                currentPipeline.Commands.Add(cmd);

                // Now add the default outputter to the end of the pipe and indicate
                // that it should handle both output and errors from the previous
                // commands. This will result in the output being written using the PSHost
                // and PSHostUserInterface classes instead of returning objects to the hosting
                // application.
                currentPipeline.Commands.Add("out-default");
                currentPipeline.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                // If there was any input specified, pass it in, otherwise just
                // execute the pipeline.
                if (input != null)
                {
                    currentPipeline.Invoke(new object[] { input });
                }
                else
                {
                    currentPipeline.Invoke();
                }
            }
            finally
            {
                // Dispose of the pipeline line and set it to null, locked because currentPipeline
                // may be accessed by the ctrl-C handler.
                currentPipeline.Dispose();
                currentPipeline = null;
            }
        }

        void HandleControlC(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Ctrl-C pressed");

            try
            {
                if (currentPipeline != null && currentPipeline.PipelineStateInfo.State == PipelineState.Running)
                    currentPipeline.Stop();

                e.Cancel = true;
            }
            catch (Exception exception)
            {
                this.myHost.UI.WriteErrorLine(exception.ToString());
            }
        }

        void Execute(string cmd)
        {
            try
            {
                // execute the command with no input...
                executeHelper(cmd, null);
            }
            catch (RuntimeException rte)
            {
                // An exception occurred that we want to display
                // using the display formatter. To do this we run
                // a second pipeline passing in the error record.
                // The runtime will bind this to the $input variable
                // which is why $input is being piped to out-default
                executeHelper("$input | out-default", rte.ErrorRecord);
            }
        }

        public void Run()
        {
            /* We use getline instead of Console.ReadLine() if the system doesn't provide a feature-rich terminal
             * emulator. If this functionality gets expanded, it should be put in a separate class which handles
             * the different platforms itself and provides common functions to be used here.
             */
            bool isWindows = Environment.OSVersion.Platform == System.PlatformID.Win32NT;
            bool? useUnixLikeConsole = GetBoolVariable(PashVariables.UseUnixLikeConsole);
            if (useUnixLikeConsole == null)
                useUnixLikeConsole = !isWindows;
            LineEditor getlineEditor = null;
            if ((bool) useUnixLikeConsole)
            {
                getlineEditor = new LineEditor("Pash");
            }

            // Set up the control-C handler.
            Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleControlC);
            Console.TreatControlCAsInput = false;

            myHost.UI.WriteLine(ConsoleColor.White, ConsoleColor.Black, "Pash - Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/");
            myHost.UI.WriteLine();

            // Loop reading commands to execute until ShouldExit is set by
            // the user calling "exit".
            while (!myHost.ShouldExit)
            {
                string prompt = Prompt();
                string cmd;

                // TODO: design this nicer (e.g. separate class(es)) as it gets more evolved
                if ((bool) useUnixLikeConsole)
                {
                    cmd = getlineEditor.Edit(prompt, "");
                }
                else
                {
                    Console.Write(prompt);
                    cmd = Console.ReadLine();
                }

                if (cmd == null)
                    continue;

                // TODO: remove the cheat - control via script and ShouldExit
                if (string.Compare(cmd.Trim(), "exit", true) == 0)
                    break;

                Execute(cmd);
            }

            // Exit with the desired exit code that was set by exit command.
            // This is set in the host by the MyHost.SetShouldExit() implementation.
            Environment.Exit(myHost.ExitCode);
        }

        internal string Prompt()
        {
            try
            {
                currentPipeline = myRunSpace.CreatePipeline("prompt");

                try
                {
                    var output = currentPipeline.Invoke();
                    if (output.Count < 1)
                    {
                        throw new Exception("'prompt' didn't return a string");
                    }
                    return output[0].ToString();
                }
                finally
                {
                    currentPipeline.Dispose();
                    currentPipeline = null;
                }
            }
            catch (Exception ex)
            {
                this.myHost.UI.WriteLine(ex.ToString());
            }
            //default, if the "prompt" function doesn't return properly
            return "PASH >";
        }

        bool? GetBoolVariable (string name)
        {
            var variable = myRunSpace.SessionStateProxy.GetVariable(name) as PSVariable;
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

        void ExecuteHelper(string cmd, object input)
        {
            // Ignore empty command lines.
            if (String.IsNullOrEmpty(cmd))
            {
                return;
            }

            // Create the pipeline object and make it available
            // to the ctrl-C handle through the currentPipeline instance
            // variable.

            currentPipeline = myRunSpace.CreatePipeline(cmd);

            // Create a pipeline for this execution. Place the result in the currentPipeline
            // instance variable so that it is available to be stopped.
            try
            {
                currentPipeline.InvokeAsync();
                if (input != null)
                {
                    currentPipeline.Input.Write(input);
                }
                currentPipeline.Input.Close();
            }
            catch
            {
                // Dispose of the pipeline line and set it to null, locked because currentPipeline
                // may be accessed by the ctrl-C handler.
                currentPipeline.Dispose();
                currentPipeline = null;
            }
        }
    }
}
