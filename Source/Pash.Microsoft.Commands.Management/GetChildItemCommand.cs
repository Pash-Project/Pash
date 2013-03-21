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

using System;
using System.Management.Automation;
using Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "ChildItem", DefaultParameterSetName = "Items")]
    public class GetChildItemCommand : CoreCommandBase
    {
        [Parameter]
        public override string[] Exclude { get; set; }

        [Parameter(Position = 1)]
        public override string Filter { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public override string[] Include { get; set; }

        [Parameter(Position = 0, ParameterSetName = "LiteralItems", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true), Alias(new string[] { "PSPath" })]
        public string[] LiteralPath { get; set; }

        [Parameter]
        public SwitchParameter Name { get; set; }

        [Parameter(Position = 0, ParameterSetName = "Items", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string[] Path { get; set; }

        [Parameter]
        public SwitchParameter Recurse { get; set; }

        public GetChildItemCommand()
        {
        }

        protected override void ProcessRecord()
        {
            if ((Path == null) || (Path.Length == 0))
            {
                Path = new string[] { string.Empty };
            }

            foreach (string str in Path)
            {
                try
                {
                    if (Name.ToBool())
                    {
                        InvokeProvider.ChildItem.GetNames(str, ReturnContainers.ReturnMatchingContainers,
                                                          Recurse.ToBool(), ProviderRuntime);
                    }
                    else
                    {
                        InvokeProvider.ChildItem.Get(str, Recurse.ToBool(), ProviderRuntime);
                    }
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "", ErrorCategory.InvalidOperation, this));
                }
            }
        }
    }
}
