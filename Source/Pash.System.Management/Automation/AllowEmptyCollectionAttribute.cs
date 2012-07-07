using System;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class AllowEmptyCollectionAttribute : CmdletMetadataAttribute
    {
        public AllowEmptyCollectionAttribute() { }
    }
}
