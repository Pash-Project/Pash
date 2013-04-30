// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
    /// <summary>
    /// A type representing information about a command's parameters.
    /// </summary>
    public class CommandParameterInfo
    {
        public ReadOnlyCollection<string> Aliases { get; private set; }
        public ReadOnlyCollection<Attribute> Attributes { get; private set; }
        public string HelpMessage { get; private set; }
        public bool IsDynamic { get; private set; }
        public bool IsMandatory { get; private set; }
        public string Name { get; private set; }
        public Type ParameterType { get; private set; }
        public int Position { get; private set; }
        public bool ValueFromPipeline { get; private set; }
        public bool ValueFromPipelineByPropertyName { get; private set; }
        public bool ValueFromRemainingArguments { get; private set; }

        // internals
        //internal CommandParameterInfo(System.Management.Automation.CompiledCommandParameter parameter, uint parameterSetFlag);

        internal CommandParameterInfo(MemberInfo info, Type paramType, ParameterAttribute paramAttr)
        {
            Name = info.Name;
            ParameterType = paramType;
            Position = paramAttr.Position;
            ValueFromPipeline = paramAttr.ValueFromPipeline;
            ValueFromPipelineByPropertyName = paramAttr.ValueFromPipelineByPropertyName;
            ValueFromRemainingArguments = paramAttr.ValueFromRemainingArguments;
            IsMandatory = paramAttr.Mandatory;

            List<Attribute> attributes = new List<Attribute>(1);
            attributes.Add(paramAttr);

            // Reflect Aliases from field/property
            AliasAttribute aliasAttr = (AliasAttribute)info.GetCustomAttributes(false).Where(i => i is AliasAttribute).FirstOrDefault();
            if (aliasAttr != null)
            {
                List<string> aliases = new List<string>(aliasAttr.AliasNames);
                Aliases = new ReadOnlyCollection<string>(aliases);
                attributes.Add(aliasAttr);
            }

            Attributes = new ReadOnlyCollection<Attribute>(attributes);
        }
    }
}
