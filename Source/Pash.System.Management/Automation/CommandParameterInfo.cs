using System;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
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

        internal CommandParameterInfo(string name, Type paramType, ParameterAttribute paramAttr)
        {
            Name = name;
            ParameterType = paramType;
            Position = paramAttr.Position;
            ValueFromPipeline = paramAttr.ValueFromPipeline;
            ValueFromPipelineByPropertyName = paramAttr.ValueFromPipelineByPropertyName;
            ValueFromRemainingArguments = paramAttr.ValueFromRemainingArguments;
            IsMandatory = paramAttr.Mandatory;

            // TODO: fill in aliases
        }
    }
}
