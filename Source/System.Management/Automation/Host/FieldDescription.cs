// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Collections.Generic;

namespace System.Management.Automation.Host
{
    public class FieldDescription
    {
        public FieldDescription(string name)
            :this(name, name, null, "", PSObject.AsPSObject(null), false, null)
        {
        }

        internal FieldDescription(string name, string label, Type paramType, string helpMessage, PSObject defaultValue,
                                  bool isMandatory, IList<Attribute> attributes)
        {
            Name = name;
            Label = label;
            HelpMessage = helpMessage;
            DefaultValue = defaultValue;
            IsMandatory = isMandatory;
            Attributes = new Collection<Attribute>(attributes);
            if (paramType != null)
            {
                SetParameterType(paramType);
            }
        }

        public Collection<Attribute> Attributes { get; private set; }
        public PSObject DefaultValue { get; set; }
        public string HelpMessage { get; set; }
        public bool IsMandatory { get; set; }
        public string Label { get; set; }

        public string Name { get; private set; }

        public string ParameterAssemblyFullName { get; private set; }

        public string ParameterTypeFullName  { get; private set; }

        public string ParameterTypeName  { get; private set; }

        internal Type ParameterType { get; private set; }

        public void SetParameterType(Type parameterType)
        {
            ParameterType = parameterType;
            var validType = ParameterType != null;
            ParameterAssemblyFullName = validType ? parameterType.AssemblyQualifiedName : "";
            ParameterTypeFullName = validType ? parameterType.FullName : "";
            ParameterTypeName = validType ? parameterType.Name : "";
        }

    }
}
