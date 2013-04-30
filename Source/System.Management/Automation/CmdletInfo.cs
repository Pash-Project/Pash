﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Pash.Implementation;

namespace System.Management.Automation
{
    /// <summary>
    /// Represents and contains information about a cmdlet.
    /// </summary>
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
                    str.AppendLine(string.Format("{0}-{1} {2}", Verb, Noun, pSet));
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

            // TODO: ensure there are no duplicate named parameters inside scope of a single parameter set.
            // TOOD: When using parameter sets, no parameter set should contain more than one positional parameter with the same position. 
            // TODO: only one parameter in a set should declare ValueFromPipeline = true. Multiple parameters may define ValueFromPipelineByPropertyName = true.

            // Add fields with ParameterAttribute
            foreach (FieldInfo filedInfo in cmdletType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                System.Diagnostics.Debug.WriteLine(filedInfo.ToString());

                object[] attributes = filedInfo.GetCustomAttributes(false);

                // Find all [Parameter] attributes on the property
                ParameterAttribute paramAttr = null;
                foreach (object attr in attributes)
                {
                    if (attr is ParameterAttribute)
                    {
                        paramAttr = (ParameterAttribute)attr;

                        CommandParameterInfo pi = new CommandParameterInfo(filedInfo, filedInfo.FieldType, paramAttr);

                        string paramSetName = paramAttr.ParameterSetName ?? ParameterAttribute.AllParameterSets;

                        if (!paramSets.ContainsKey(paramSetName))
                        {
                            paramSets.Add(paramSetName, new Collection<CommandParameterInfo>());
                        }

                        Collection<CommandParameterInfo> paramSet = paramSets[paramSetName];
                        paramSet.Add(pi);
                    }
                }
            }

            // Add properties with ParameterAttribute
            foreach (PropertyInfo filedInfo in cmdletType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                System.Diagnostics.Debug.WriteLine(filedInfo.ToString());

                object[] attributes = filedInfo.GetCustomAttributes(false);

                // Get info for the setter and getter
                MethodInfo getter = filedInfo.GetAccessors().FirstOrDefault(i => i.IsSpecialName && i.Name.StartsWith("get_"));
                MethodInfo setter = filedInfo.GetSetMethod();

                // Find all [Parameter] attributes on the property
                ParameterAttribute paramAttr = null;
                foreach (object attr in attributes)
                {
                    if (attr is ParameterAttribute)
                    {
                        paramAttr = (ParameterAttribute)attr;

                        // if ValueFromPipeline or ValueFromPipelineByPropertyName is specified the property must have a public getter
                        if ((paramAttr.ValueFromPipeline || paramAttr.ValueFromPipelineByPropertyName) && (getter == null || !getter.IsPublic))
                        {
                            break;
                        }

                        // The property must have a public setter
                        if (setter == null || !setter.IsPublic)
                        {
                            break;
                        }

                        CommandParameterInfo pi = new CommandParameterInfo(filedInfo, filedInfo.PropertyType, paramAttr);

                        string paramSetName = paramAttr.ParameterSetName ?? ParameterAttribute.AllParameterSets;

                        if (!paramSets.ContainsKey(paramSetName))
                        {
                            paramSets.Add(paramSetName, new Collection<CommandParameterInfo>());
                        }

                        Collection<CommandParameterInfo> paramSet = paramSets[paramSetName];
                        paramSet.Add(pi);
                    }
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

                // If a parameter set is not specified for a parmeter, then the parameter belongs to all the parameter sets,
                // therefore if this is not the AllParameterSets Set then add all parameters from the AllParameterSets Set to it...
                if (string.Compare(paramSetName, ParameterAttribute.AllParameterSets) != 0 && paramSets.ContainsKey(ParameterAttribute.AllParameterSets))
                {
                    foreach (CommandParameterInfo cpi in paramSets[ParameterAttribute.AllParameterSets])
                    {
                        paramSets[paramSetName].Add(cpi);
                    }
                }
            }

            return new ReadOnlyCollection<CommandParameterSetInfo>(paramSetInfo);
        }

        internal CommandParameterSetInfo GetParameterSetByName(string strParamSetName)
        {
            foreach (CommandParameterSetInfo paramSetInfo in ParameterSets)
            {
                if (string.Compare(strParamSetName, paramSetInfo.Name, true) == 0)
                    return paramSetInfo;
            }

            return null;
        }

        internal CommandParameterSetInfo GetDefaultParameterSet()
        {
            foreach (CommandParameterSetInfo paramSetInfo in ParameterSets)
            {
                if (paramSetInfo.IsDefault)
                    return paramSetInfo;
            }

            return null;
        }
    }
}
