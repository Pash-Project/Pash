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

namespace Pash
{
    // TODO: fix all the assembly attributes
    class Program
    {
        static void Main(string[] args)
        {

            /*
            PashHost host = new PashHost();

            using (Runspace space = RunspaceFactory.CreateRunspace(host))
            {
                space.Open();

                // Create the pipeline.
                Pipeline pipe = space.CreatePipeline();

                // Add the script we want to run. This script does two things. 
                // First, it runs the Get-Process cmdlet with the cmdlet output 
                // sorted by handle count. Second, the GetDate cmdlet is piped 
                // to the Out-String cmdlet so that we can see the
                // date displayed in German.

                pipe.Commands.AddScript(@"
                    get-process | sort handlecount
                    # This should display the date in German...
                    get-date | out-string
                    ");

                // Add the default outputter to the end of the pipe and indicate
                // that it should handle both output and errors from the previous
                // commands. This will result in the output being written using the PSHost
                // and PSHostUserInterface classes instead of returning objects to the hosting
                // application.
                pipe.Commands.Add("out-default");
                pipe.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                Collection<PSObject> results = pipe.Invoke();

                foreach (PSObject result in results)
                {
                    Console.WriteLine(result);
                }

                System.Console.WriteLine("Hit any key to exit...");
                System.Console.ReadKey();
            }
            */

            FullHost p = new FullHost();
            p.Run();
        }
    }
}
