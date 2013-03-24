// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
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
            return Parameters.SingleOrDefault(parameter => parameter.Position == position);
        }

        internal CommandParameterInfo GetParameterByName(string name)
        {
            return Parameters.SingleOrDefault(parameter => string.Equals(parameter.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
