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

        // internals
        //internal CommandParameterSetInfo(string name, bool isDefaultParameterSet, uint parameterSetFlag, MergedCommandParameterMetadata parameterMetadata)
        internal CommandParameterSetInfo(string name, bool isDefaultParameterSet, Collection<CommandParameterInfo> paramsInfo)
        {
            Name = name;
            IsDefault = isDefaultParameterSet;

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

        internal CommandParameterInfo LookupParameter(string name)
        {
            // TODO: Exception should include all possible matches
            CommandParameterInfo found = null;
            foreach (CommandParameterInfo parameter in Parameters)
            {
                if (parameter.Name.StartsWith(name, StringComparison.CurrentCultureIgnoreCase) ||
                    (parameter.Aliases != null && parameter.Aliases.Where(a => a.StartsWith(name, StringComparison.CurrentCultureIgnoreCase)).Count() > 0))
                {
                    // If match already found, name is ambiguous.
                    if (found != null)
                    {
                        // AmbiguousParameter
                        throw new ParameterBindingException("Supplied parmameter '" + name + "' is ambiguous, possibilities include '" + found.Name + "' and '" + parameter.Name + "'" );
                    }
                    found = parameter;
                }
            }

            return found;
        }

        internal IDictionary<string, CommandParameterInfo> LookupAllParameters(IEnumerable<string> names)
        {
            var lookupDictionary = new Dictionary<string, CommandParameterInfo> ();
            foreach (string name in names) {
                lookupDictionary.Add (name, this.LookupParameter(name));
            }
            return lookupDictionary;
        }

        public override string ToString()
        {
            return String.Format("{0}({1} parameters)", Name, Parameters.Count);
        }
    }
}
