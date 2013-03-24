// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class AllowEmptyStringAttribute : CmdletMetadataAttribute
    {
        public AllowEmptyStringAttribute()
        {
        }
    }
}
