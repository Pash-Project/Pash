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
    public class CommandParameterInfo : IEquatable<CommandParameterInfo>
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

        internal MemberInfo MemberInfo { get; private set; }

        internal CommandParameterInfo(MemberInfo info, Type paramType, ParameterAttribute paramAttr)
        {
            MemberInfo = info;
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
            else
            {
                Aliases = new ReadOnlyCollection<string>(new List<string>());
            }

            Attributes = new ReadOnlyCollection<Attribute>(attributes);
        }

        public bool Equals(CommandParameterInfo other)
        {
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }
            return Name.Equals(other.Name) && ParameterType.Equals(other.ParameterType);
        }

        public override bool Equals(object obj)
        {
            var otherParamInfo = obj as CommandParameterInfo;
            if (obj == null)
            {
                throw new ArgumentException("Equality check with null or different obejct type");
            }
            return Equals(otherParamInfo);
        }

        public override string ToString()
        {
            return string.Format("[{0}:{1}]", Name, ParameterType.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ ParameterType.GetHashCode();
        }
    }
}
