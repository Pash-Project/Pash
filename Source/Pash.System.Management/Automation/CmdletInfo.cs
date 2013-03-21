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
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Pash.Implementation;

namespace System.Management.Automation
{
    public class CmdletInfo : CommandInfo
    {
        public string HelpFile { get; private set; }
        public Type ImplementingType { get; private set; }
        public string Noun { get; private set; }
        public PSSnapInInfo PSSnapIn { get; private set; }
        public string Verb { get; private set; }
        public ReadOnlyCollection<CommandParameterSetInfo> ParameterSets { get; private set; }

        private ExecutionContext _context;

        internal CmdletInfo(string name, Type implementingType, string helpFile, PSSnapInInfo PSSnapin, ExecutionContext context)
            : base(name, CommandTypes.Cmdlet)
        {
            int i = name.IndexOf('-');
            if (i == -1)
            {
                throw new Exception("InvalidCmdletNameFormat " + name);
            }
            Verb = name.Substring(0, i);
            Noun = name.Substring(i + 1);
            ImplementingType = implementingType;
            HelpFile = helpFile;
            PSSnapIn = PSSnapin;
            _context = context;
            ParameterSets = GetParameterSetInfo(implementingType);
        }

        public override string Definition
        {
            get
            {
                StringBuilder str = new StringBuilder();
                foreach (CommandParameterSetInfo pSet in ParameterSets)
                {
                    str.AppendLine(string.Format(string.Format("{0}-{1} {2}", Verb, Noun, pSet.ToString())));
                }

                return str.ToString();
            }
        }

        // internals
        //internal CmdletInfo CreateGetCommandCopy(CmdletInfo cmdletInfo, object[] arguments);
        //internal object[] Arguments { set; get; }
        //internal string FullName { get; }
        //internal bool IsGetCommandCopy { set; get; }

        /// <remarks>
        /// From MSDN article: http://msdn2.microsoft.com/en-us/library/ms714348(VS.85).aspx
        /// 
        /// * A cmdlet can have any number of parameters. However, for a better user experience the number 
        ///   of parameters should be limited when possible. 
        /// * Parameters must be declared on public non-static fields or properties. It is preferred for 
        ///   parameters to be declared on properties. The property must have a public setter, and if ValueFromPipeline 
        ///   or ValueFromPipelineByPropertyName is specified the property must have a public getter.
        /// * When specifying positional parameters, note the following.
        ///   * Limit the number of positional parameters in a parameter set to less than five if possible.
        ///   * Positional parameters do not have to be sequential; positions 5, 100, 250 works the same as positions 0, 1, 2.
        /// * When the Position keyword is not specified, the cmdlet parameter must be referenced by its name.
        /// * When using parameter sets, no parameter set should contain more than one positional parameter with the same position. 
        /// * In addition, only one parameter in a set should declare ValueFromPipeline = true. Multiple parameters may define ValueFromPipelineByPropertyName = true.
        /// 
        /// </remarks>
        private static ReadOnlyCollection<CommandParameterSetInfo> GetParameterSetInfo(Type cmdletType)
        {
            Dictionary<string, Collection<CommandParameterInfo>> paramSets = new Dictionary<string, Collection<CommandParameterInfo>>(StringComparer.CurrentCultureIgnoreCase);

            // Extract all the public parameters from the Cmdlet
            /* TODO: gather all the field parameters
            foreach(var filedInfo in cmdletType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                System.Diagnostics.Debug.WriteLine(filedInfo.ToString());
            }
            */

            foreach (PropertyInfo filedInfo in cmdletType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                System.Diagnostics.Debug.WriteLine(filedInfo.ToString());

                object[] attributes = filedInfo.GetCustomAttributes(false);

                // Find if there are any [Parameter] attributes on the property
                ParameterAttribute paramAttr = null;
                foreach (object attr in attributes)
                {
                    if (attr is ParameterAttribute)
                    {
                        paramAttr = (ParameterAttribute)attr;
                        break;
                    }
                }

                // TODO: make sure that the PropertyInfo.GetAccessors() returns the appropriate set of accessors

                if (paramAttr != null)
                {
                    CommandParameterInfo pi = new CommandParameterInfo(filedInfo.Name, filedInfo.PropertyType, paramAttr);

                    string paramSetName = paramAttr.ParameterSetName ?? ParameterAttribute.AllParameterSets;

                    if (!paramSets.ContainsKey(paramSetName))
                    {
                        paramSets.Add(paramSetName, new Collection<CommandParameterInfo>());
                    }

                    Collection<CommandParameterInfo> paramSet = paramSets[paramSetName];
                    paramSet.Add(pi);
                }
            }

            // Create param-sets collection
            Collection<CommandParameterSetInfo> paramSetInfo = new Collection<CommandParameterSetInfo>();

            // TODO: find the name of the default param set
            string strDefaultParameterSetName = ParameterAttribute.AllParameterSets;
            object[] cmdLetAttrs = cmdletType.GetCustomAttributes(typeof(CmdletAttribute), false);
            if (cmdLetAttrs.Length > 0)
                strDefaultParameterSetName = ((CmdletAttribute)cmdLetAttrs[0]).DefaultParameterSetName ?? strDefaultParameterSetName;

            foreach (string paramSetName in paramSets.Keys)
            {
                bool bIsDefaultParamSet = paramSetName == strDefaultParameterSetName;

                paramSetInfo.Add(new CommandParameterSetInfo(paramSetName, bIsDefaultParamSet, paramSets[paramSetName]));
            }

            return new ReadOnlyCollection<CommandParameterSetInfo>(paramSetInfo);
        }

        internal CommandParameterSetInfo GetParameterSetByName(string strParamSetName)
        {
            return ParameterSets.SingleOrDefault(paramSetInfo => string.Compare(strParamSetName, paramSetInfo.Name, true) == 0);
        }

        internal CommandParameterSetInfo GetDefaultParameterSet()
        {
            return ParameterSets.SingleOrDefault(paramSetInfo => paramSetInfo.IsDefault);
        }
    }
}
