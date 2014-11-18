// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    /// <summary>
    /// A type representing the set of command parameters for a cmdlet.
    /// </summary>
    public class CommandParameterSetInfo
    {
        public bool IsDefault { get; private set; }
        public string Name { get; private set; }
        public ReadOnlyCollection<CommandParameterInfo> Parameters { get; private set; }
        public bool IsAllParameterSets { get; private set; }

        // internals
        //internal CommandParameterSetInfo(string name, bool isDefaultParameterSet, uint parameterSetFlag, MergedCommandParameterMetadata parameterMetadata)
        internal CommandParameterSetInfo(string name, bool isDefaultParameterSet, Collection<CommandParameterInfo> paramsInfo)
        {
            Name = name;
            IsDefault = isDefaultParameterSet;
            IsAllParameterSets = Name.Equals(ParameterAttribute.AllParameterSets);

            // TODO: fill in the parameters info
            Parameters = new ReadOnlyCollection<CommandParameterInfo>(paramsInfo);
        }

        internal CommandParameterInfo GetParameterByName(string name)
        {
            foreach (CommandParameterInfo parameter in Parameters)
            {
                if (string.Equals(parameter.Name, name, StringComparison.CurrentCultureIgnoreCase))
                    return parameter;
            }

            return null;
        }

        internal bool Contains(CommandParameterInfo param)
        {
            return Parameters.Contains(param);
        }

        public override string ToString()
        {
            return String.Format("{0}({1} parameters)", Name, Parameters.Count);
        }
    }
}
