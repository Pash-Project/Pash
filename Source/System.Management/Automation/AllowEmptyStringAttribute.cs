// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    /// <summary>
    /// Attribute that allows an empty string as an arguement.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class AllowEmptyStringAttribute : CmdletMetadataAttribute
    {
        public AllowEmptyStringAttribute()
        {
        }
    }
}
