// Copyright (C) Pash Contributors (https://github.com/Pash-Project/Pash/blob/master/AUTHORS.md). All Rights Reserved.

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

using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Location", DefaultParameterSetName = "Location")]
    public class GetLocationCommand : DriveMatchingCoreCommandBase
    {
        [Parameter(ParameterSetName = "Location", ValueFromPipelineByPropertyName = true)]
        public string[] PSDrive { get; set; }

        [Parameter(ParameterSetName = "Location", ValueFromPipelineByPropertyName = true)]
        public string[] PSProvider { get; set; }

        [Parameter(ParameterSetName = "Stack")]
        public SwitchParameter Stack { get; set; }

        [Parameter(ParameterSetName = "Stack", ValueFromPipelineByPropertyName = true)]
        public string[] StackName { get; set; }

        public GetLocationCommand()
        {
            PSProvider = new string[0];
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "Stack")
            {
                // TODO: implement the location stack manipulations
                return;
            }
            else
            {
                if ((PSDrive != null) && (PSDrive.Length > 0))
                {
                    // If location is requested for a specific drive
                    foreach (string str in PSDrive)
                    {
                        List<PSDriveInfo> list = GetDrivesByName(str, PSProvider);

                        foreach (PSDriveInfo pdi in list)
                        {
                            WriteObject(new PathInfo(pdi, pdi.Provider, PathIntrinsics.MakePath(pdi.CurrentLocation, pdi), SessionState));
                        }
                    }
                }
                else if ((PSProvider != null) && (PSProvider.Length > 0))
                {
                    // If location was requested for a specific provider
                    foreach (string proverName in PSProvider)
                    {
                        foreach (ProviderInfo pi in SessionState.Provider.GetAll())
                        {
                            if (pi.IsNameMatch(proverName))
                            {
                                WriteObject(SessionState.Path.CurrentProviderLocation(pi.FullName));
                            }
                        }
                    }
                }
                else
                {
                    // If nothing specific was requested - return the current location
                    WriteObject(SessionState.Path.CurrentLocation);
                }
            }
        }
    }
}
