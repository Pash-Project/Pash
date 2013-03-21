// Copyright (C) Pash Contributors. All Rights Reserved. See https://github.com/Pash-Project/Pash/

#region BSD License
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are
// those of the authors and should not be interpreted as representing official
// policies, (either expressed or implied, of the FreeBSD Project.
#endregion

#region GPL License
// This file is part of Pash.
//
// Pash is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// Pash is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along
// with Pash.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.ObjectModel;
using Pash.Implementation;
using System.Reflection;
using System.IO;

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
            try
            {
                Execute("prompt | write-host -nonewline");

            }
            catch (Exception ex)
            {

                this.myHost.UI.WriteLine(ex.ToString());
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
