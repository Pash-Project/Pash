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
using System.Collections;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Set", "Variable", SupportsShouldProcess = true)]
    public sealed class SetVariableCommand : PSCmdlet
    {
        [Parameter]
        public string Description { get; set; }

        [Parameter]
        public string[] Exclude { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        [Parameter]
        public string[] Include { get; set; }

        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter]
        public ScopedItemOptions Option { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public object Value { get; set; }

        private PSObject _default;
        private ArrayList _values;

        //private bool nameIsFormalParameter;
        //private ScopedItemOptions? options;
        //private bool valueIsFormalParameter;
        //private ArrayList valueList;

        public SetVariableCommand()
        {
            _default = new PSObject();
            Value = _default;
            _values = new ArrayList();
        }

        protected override void ProcessRecord()
        {
            if (Value != _default)
            {
                _values.Add(Value);
            }
        }

        protected override void EndProcessing()
        {
            foreach (string name in Name)
            {
                foreach (object value in _values)
                {
                    PSVariable variable = SessionState.PSVariable.Get(name);

                    if (variable == null)
                    {
                        variable = new PSVariable(name, value);
                    }
                    else
                    {
                        variable.Value = value;
                    }

                    SessionState.PSVariable.Set(variable);
                }
            }
        }

        private void SetVariable(string[] varNames, object varValue)
        {
            throw new NotImplementedException();
        }
    }
}
