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
using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using Pash.Implementation;
using System.Management.Automation;

namespace System.Management.Automation
{
    internal class CommandProcessor : CommandProcessorBase
    {
        internal Cmdlet Command { get; set; }
        readonly CmdletInfo _cmdletInfo;
        private bool _beganProcessing;

        internal override CommandInfo CommandInfo
        {
            get { return this._cmdletInfo; }
        }

        public CommandProcessor(CmdletInfo cmdletInfo)
        {
            _cmdletInfo = cmdletInfo;
            _beganProcessing = false;
        }

        internal override ICommandRuntime CommandRuntime
        {
            get
            {
                return Command.CommandRuntime;
            }
            set
            {
                Command.CommandRuntime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <remarks>
        /// All abount Cmdlet parameters: http://msdn2.microsoft.com/en-us/library/ms714433(VS.85).aspx
        /// </remarks>
        internal override void BindArguments(PSObject obj)
        {
            if ((obj == null) && (Parameters.Count == 0))
                return;

            // TODO: bind the arguments to the parameters
            CommandParameterSetInfo paramSetInfo = _cmdletInfo.GetDefaultParameterSet();
            // TODO: refer to the Command._ParameterSetName for a param set name

            if (obj != null)
            {
                foreach (CommandParameterInfo paramInfo in paramSetInfo.Parameters)
                {
                    if (paramInfo.ValueFromPipeline)
                    {
                        BindArgument(paramInfo.Name, obj, paramInfo.ParameterType);
                    }
                }
            }

            if (Parameters.Count > 0)
            {
                // bind by position location
                for (int i = 0; i < Parameters.Count; i++)
                {
                    CommandParameterInfo paramInfo = null;

                    CommandParameter parameter = Parameters[i];

                    if (string.IsNullOrEmpty(parameter.Name))
                    {
                        paramInfo = paramSetInfo.GetParameterByPosition(i);

                        if (paramInfo != null)
                        {
                            BindArgument(paramInfo.Name, parameter.Value, paramInfo.ParameterType);
                        }
                    }
                    else
                    {
                        paramInfo = paramSetInfo.GetParameterByName(parameter.Name);

                        if (paramInfo != null)
                        {
                            BindArgument(paramInfo.Name, parameter.Value, paramInfo.ParameterType);
                        }
                    }
                }
            }
        }

        private void BindArgument(string name, object value, Type type)
        {
            // TODO: extract this into a method
            PropertyInfo propertyInfo = Command.GetType().GetProperty(name, type);

            // TODO: make this generic
            if (propertyInfo.PropertyType == typeof(PSObject[]))
            {
                propertyInfo.SetValue(Command, new[] { PSObject.AsPSObject(value) }, null);
            }

            else if (propertyInfo.PropertyType == typeof(String[]))
            {
                propertyInfo.SetValue(Command, new[] { value.ToString() }, null);
            }

            else if (propertyInfo.PropertyType == typeof(String))
            {
                propertyInfo.SetValue(Command, value.ToString(), null);
            }

            else if (propertyInfo.PropertyType == typeof(PSObject))
            {
                propertyInfo.SetValue(Command, PSObject.AsPSObject(value), null);
            }

            else if (propertyInfo.PropertyType == typeof(SwitchParameter))
            {
                propertyInfo.SetValue(Command, new SwitchParameter(true), null);
            }

            else if (propertyInfo.PropertyType == typeof(Object[]))
            {
                propertyInfo.SetValue(Command, new[] { value }, null);
            }

            else
            {
                propertyInfo.SetValue(Command, value, null);
            }
        }

        internal override void Initialize()
        {
            try
            {
                Cmdlet cmdlet = (Cmdlet)Activator.CreateInstance(_cmdletInfo.ImplementingType);

                cmdlet.CommandInfo = _cmdletInfo;
                cmdlet.ExecutionContext = base.ExecutionContext;
                Command = cmdlet;
            }
            catch (Exception e)
            {
                // TODO: work out the failure
                System.Console.WriteLine(e);
            }
        }

        internal override void ProcessRecord()
        {
            // TODO: initialize Cmdlet parameters
            if (!_beganProcessing)
            {
                Command.DoBeginProcessing();
                _beganProcessing = true;
            }

            Command.DoProcessRecord();
        }

        internal void ProcessObject(object inObject)
        {
            /*
            PSObject inPsObject = null;
            if (inputObject != null)
            {
                inPsObject = PSObject.AsPSObject(inObject);
            }
            */
        }

        internal override void Complete()
        {
            Command.DoEndProcessing();
        }
    }
}
