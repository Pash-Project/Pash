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
using System.Management.Automation;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "Member")]
    public class GetMemberCommand : PSCmdlet
    {
        private HybridDictionary _membersCollection;

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Alias(new string[] { "Type" }), Parameter]
        public PSMemberTypes MemberType { get; set; }

        [Parameter(Position = 0)]
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter Static { get; set; }

        public GetMemberCommand()
        {
            MemberType = PSMemberTypes.All;
            _membersCollection = new HybridDictionary();
        }

        protected override void ProcessRecord()
        {
            // MUST: implement the InputObject binding in the pipe
            if ((InputObject == null) || (InputObject == AutomationNull.Value))
                return;

            string fullName;

            // TODO: deal with Static

            if (InputObject.TypeNames.Count != 0)
            {
                fullName = this.InputObject.TypeNames[0];
            }
            else
            {
                fullName = "<null>";
            }

            if (!_membersCollection.Contains(fullName))
            {
                _membersCollection.Add(fullName, "");

                foreach (string name in Name)
                {
                    ReadOnlyPSMemberInfoCollection<PSMemberInfo> infos;

                    // TODO: deal with Static members
                    infos = InputObject.Members.Match(name, this.MemberType);

                    List<MemberDefinition> members = new List<MemberDefinition>();
                    foreach (PSMemberInfo info in infos)
                    {
                        members.Add(new MemberDefinition(fullName, info.Name, info.MemberType, info.ToString()));
                    }

                    members.Sort((def1, def2) =>
                    {
                        int diff =
                            string.Compare(def1.MemberType.ToString(), def2.MemberType.ToString(),
                                           StringComparison.CurrentCultureIgnoreCase);
                        if (diff != 0)
                        {
                            return diff;
                        }
                        return
                            string.Compare(def1.Name, def2.Name,
                                           StringComparison.CurrentCultureIgnoreCase);
                    });

                    foreach (MemberDefinition definition in members)
                    {
                        WriteObject(definition);
                    }
                }
            }
        }

        protected override void EndProcessing()
        {
            if (_membersCollection.Count == 0)
            {
                // TODO: WriteError
                throw new Exception("No object specified");
            }
        }
    }
}
