// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Pash;
using Pash.Implementation;

namespace System.Management.Automation
{
    /// <summary>
    /// Represents and contains information about a cmdlet.
    /// </summary>
    public class CmdletInfo : CommandInfo, IScopedItem
    {
        public string HelpFile { get; private set; }
        public Type ImplementingType { get; private set; }
        public string Noun { get; private set; }
        public PSSnapInInfo PSSnapIn { get; private set; }
        public string Verb { get; private set; }
        public ReadOnlyCollection<CommandParameterSetInfo> ParameterSets { get; private set; }

        public override ReadOnlyCollection<PSTypeName> OutputType {
            get { return outputType; }
        }

        internal Dictionary<string, CommandParameterInfo> ParameterInfoLookupTable { get; private set; }

        private Exception _validationException;
        private ReadOnlyCollection<PSTypeName> outputType;

        internal CmdletInfo(string name, Type implementingType, string helpFile)
            : this(name, implementingType, helpFile, null, null)
        {
        }

        internal CmdletInfo(string name, Type implementingType, string helpFile, PSSnapInInfo snapin)
            : this(name, implementingType, helpFile, snapin, null)
        {
        }

        internal CmdletInfo(string name, Type implementingType, string helpFile, PSModuleInfo module)
            : this(name, implementingType, helpFile, null, module)
        {

        }

        internal CmdletInfo(string name, Type implementingType, string helpFile, PSSnapInInfo snapin, PSModuleInfo module)
            : base(name, CommandTypes.Cmdlet)
        {
            int i = name.IndexOf('-');
            if (i == -1)
            {
                throw new Exception("InvalidCmdletNameFormat " + name);
            }
            ParameterInfoLookupTable = new Dictionary<string, CommandParameterInfo>(StringComparer.CurrentCultureIgnoreCase);
            Verb = name.Substring(0, i);
            Noun = name.Substring(i + 1);
            ImplementingType = implementingType;
            HelpFile = helpFile;
            _validationException = null;
            PSSnapIn = snapin;
            Module = module;
            InitializeParameterSetInfo(implementingType);
            InitializeOutputTypes(implementingType);
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

        internal override void Validate()
        {
            if (_validationException != null)
            {
                throw _validationException;
            }
        }

        private void RegisterParameterInLookupTable(CommandParameterInfo parameterInfo)
        {
            // also add it to lookuptable and check for unuque names/aliases
            var allNames = parameterInfo.Aliases.ToList();
            allNames.Add(parameterInfo.Name);
            foreach (var curName in allNames)
            {
                if (ParameterInfoLookupTable.ContainsKey(curName))
                {
                    // save exception to be thrown when this object is validated, not now
                    var msg = String.Format("The name or alias '{0}' is used multiple times.", curName);
                    _validationException = new MetadataException(msg);
                    continue;
                }
                ParameterInfoLookupTable[curName] = parameterInfo;
            }
        }

        internal CommandParameterInfo LookupParameter(string name)
        {
            // check for complete name first
            if (ParameterInfoLookupTable.ContainsKey(name))
            {
                return ParameterInfoLookupTable[name];
            }

            // if we didn't find it by name or alias, try to find it by prefix
            var candidates = (from key in ParameterInfoLookupTable.Keys
                              where key.StartsWith(name, StringComparison.OrdinalIgnoreCase)
                              select key).ToList();
            if (candidates.Count < 1)
            {
                var msg = String.Format("No parameter was found that matches the name or alias '{0}'.", name);
                throw new ParameterBindingException(msg, "ParameterNotFound");
            }
            if (candidates.Count > 1)
            {
                var msg = String.Format("Supplied parmameter '{0}' is ambiguous, possibilities are: {1}",
                              name, String.Join(", ", candidates));
                throw new ParameterBindingException(msg, "AmbiguousParameter");
            }

            return ParameterInfoLookupTable[candidates[0]];
        }

        private void AddParameterToParameterSet(Dictionary<string, Collection<CommandParameterInfo>> paramSets,
                                                CommandParameterInfo paramInfo)
        {
            var paramSetName = paramInfo.ParameterSetName;
            // create the parameter set if it doesn't exist, yet
            if (!paramSets.ContainsKey(paramSetName))
            {
                paramSets.Add(paramSetName, new Collection<CommandParameterInfo>());
            }
            Collection<CommandParameterInfo> paramSet = paramSets[paramSetName];
            // actually add parameter to the set
            paramSet.Add(paramInfo);
        }

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
        private void InitializeParameterSetInfo(Type cmdletType)
        {
            Dictionary<string, Collection<CommandParameterInfo>> paramSets = new Dictionary<string, Collection<CommandParameterInfo>>(StringComparer.CurrentCultureIgnoreCase);

            // TODO: When using parameter sets, no parameter set should contain more than one positional parameter with the same position. 
            // TODO: If not parameters have a position declared then positions for all the parameters should be automatically declaredin the order they are specified
            // TODO: Only one parameter in a set should declare ValueFromRemainingArguments = true
            // TODO: Only one parameter in a set should declare ValueFromPipeline = true. Multiple parameters may define ValueFromPipelineByPropertyName = true.
            // TODO: Currently due to the way parameters are loaded into sets from all set at the end the parameter end up in incorrect order.

            // get the name of the default parameter set
            string strDefaultParameterSetName = null;
            object[] cmdLetAttrs = cmdletType.GetCustomAttributes(typeof(CmdletAttribute), false);
            if (cmdLetAttrs.Length > 0)
            {
                strDefaultParameterSetName = ((CmdletAttribute)cmdLetAttrs[0]).DefaultParameterSetName;
                // If a default set is specified, it has to exist, even if it's empty.
                // See NonExisitingDefaultParameterSetIsEmptyPatameterSet reference test
                if (!String.IsNullOrEmpty(strDefaultParameterSetName))
                {
                    paramSets.Add(strDefaultParameterSetName, new Collection<CommandParameterInfo>());
                }
            }

            // always have a parameter set for all parameters. even if we don't have any parameters or no parameters
            // that are in all sets. This will nevertheless save various checks
            if (!paramSets.ContainsKey(ParameterAttribute.AllParameterSets)) {
                paramSets.Add(ParameterAttribute.AllParameterSets, new Collection<CommandParameterInfo>());
            }

            var parameterDiscovery = new CommandParameterDiscovery(cmdletType);
            // first add the parameters to the parameter sets
            foreach (var parameter in parameterDiscovery.AllParameters)
            {
                AddParameterToParameterSet(paramSets, parameter);
            }

            // now add each parameter member to the lookup table
            foreach (var namedParam in parameterDiscovery.NamedParameters.Values)
            {
                RegisterParameterInLookupTable(namedParam);
            }

            // Create param-sets collection. Add the parameters from the AllParameterSets set to all other sets
            Collection<CommandParameterSetInfo> paramSetInfo = new Collection<CommandParameterSetInfo>();
            foreach (string paramSetName in paramSets.Keys)
            {
                // If a parameter set is not specified for a parmeter, then the parameter belongs to all the parameter sets,
                // therefore if this is not the AllParameterSets Set then add all parameters from the AllParameterSets Set to it...
                if (!paramSetName.Equals(ParameterAttribute.AllParameterSets))
                {
                    foreach (CommandParameterInfo cpi in paramSets[ParameterAttribute.AllParameterSets])
                    {
                        paramSets[paramSetName].Add(cpi);
                    }
                }
                // add the set to the setInfo collection
                bool bIsDefaultParamSet = paramSetName.Equals(strDefaultParameterSetName);
                paramSetInfo.Add(new CommandParameterSetInfo(paramSetName, bIsDefaultParamSet, paramSets[paramSetName]));
            }

            ParameterSets = new ReadOnlyCollection<CommandParameterSetInfo>(paramSetInfo);

            // last, but not least, at all common parameters to all parameter sets
            AddParametersToAllSets(CommonCmdletParameters.ParameterDiscovery);
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
            // TODO: get by name
            foreach (CommandParameterSetInfo paramSetInfo in ParameterSets)
            {
                if (paramSetInfo.IsDefault)
                    return paramSetInfo;
            }

            return null;
        }

        internal ReadOnlyCollection<CommandParameterSetInfo> GetNonDefaultParameterSets()
        {
            return new ReadOnlyCollection<CommandParameterSetInfo>(ParameterSets.Where(x => !x.IsDefault).ToList());
        }

        internal void AddParametersToAllSets(CommandParameterDiscovery discovery)
        {
            // add the parameters to all sets
            var updatedParameterSets = new List<CommandParameterSetInfo>();
            foreach (CommandParameterSetInfo parameterSet in ParameterSets)
            {
                List<CommandParameterInfo> updatedParameters = parameterSet.Parameters.ToList();

                updatedParameters.AddRange(discovery.AllParameters);

                updatedParameterSets.Add(new CommandParameterSetInfo(
                    parameterSet.Name,
                    parameterSet.IsDefault,
                    new Collection<CommandParameterInfo>(updatedParameters)));
            }

            ParameterSets = new ReadOnlyCollection<CommandParameterSetInfo>(updatedParameterSets);

            // register all parameter names in the lookup table
            foreach (var namedParameter in discovery.NamedParameters.Values)
            {
                RegisterParameterInLookupTable(namedParameter);
            }
        }

        private void InitializeOutputTypes(Type cmdletType)
        {
            var types = new List<PSTypeName>();
            foreach (OutputTypeAttribute attribute in cmdletType.GetCustomAttributes(typeof(OutputTypeAttribute), false))
            {
                types.AddRange(attribute.Type);
            }
            outputType = new ReadOnlyCollection<PSTypeName>(types);
        }

        #region IScopedItem Members

        public string ItemName
        {
            get { return Name; }
        }

        public ScopedItemOptions ItemOptions
        {
            get { return ScopedItemOptions.None; }
            set { throw new NotImplementedException("Setting scope options for cmdlets is not supported"); }
        }
        #endregion

    }
}
