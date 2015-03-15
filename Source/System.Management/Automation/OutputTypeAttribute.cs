// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class OutputTypeAttribute : CmdletMetadataAttribute
    {
        public OutputTypeAttribute(params string[] type)
        {
        }

        public OutputTypeAttribute(params Type[] type)
        {
            Type = type.Select(t => new PSTypeName(t)).ToArray();
        }

        public string[] ParameterSetName { get; set; }
        public string ProviderCmdlet { get; set; }
        public PSTypeName[] Type { get; private set; }
    }
}
