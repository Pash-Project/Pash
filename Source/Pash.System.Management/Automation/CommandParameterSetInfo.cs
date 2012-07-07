using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    public class CommandParameterSetInfo
    {
        public bool IsDefault { get; private set; }
        public string Name { get; private set; }
        public ReadOnlyCollection<CommandParameterInfo> Parameters { get; private set; }

        public override string ToString() { throw new NotImplementedException(); }

        // internals
        //internal CommandParameterSetInfo(string name, bool isDefaultParameterSet, uint parameterSetFlag, MergedCommandParameterMetadata parameterMetadata)
        internal CommandParameterSetInfo(string name, bool isDefaultParameterSet, Collection<CommandParameterInfo> paramsInfo)
        {
            Name = name;
            IsDefault = isDefaultParameterSet;

            // TODO: fill in the parameters info
            Parameters = new ReadOnlyCollection<CommandParameterInfo>(paramsInfo);
        }

        internal CommandParameterInfo GetParameterByPosition(int position)
        {
            foreach (CommandParameterInfo parameter in Parameters)
            {
                if (parameter.Position == position)
                    return parameter;
            }

            return null;
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
    }
}
