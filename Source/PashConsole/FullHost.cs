using System;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.ObjectModel;
using Pash.Implementation;

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
            // Set up the control-C handler.
            Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleControlC);
            Console.TreatControlCAsInput = false;

            myHost.UI.WriteLine(ConsoleColor.White, ConsoleColor.Black, "Pash (PowerShell open source reimplementation)");
            myHost.UI.WriteLine(ConsoleColor.White, ConsoleColor.Black, "Implemented by IgorM @ http://IgorShare.WordPress.com");
            myHost.UI.WriteLine();

            // Loop reading commands to execute until ShouldExit is set by
            // the user calling "exit".
            while (!myHost.ShouldExit)
            {
                Prompt();

                string cmd = Console.ReadLine();

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

        internal void Prompt()
        {
            // This should be "prompt | write-host -nonewline", but that doesn't work yet.
            Execute("prompt");
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
