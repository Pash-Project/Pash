using System;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public abstract class ValidateArgumentsAttribute : CmdletMetadataAttribute
    {
        protected ValidateArgumentsAttribute() { }

        protected abstract void Validate(object arguments, EngineIntrinsics engineIntrinsics);

        // internals
        internal void InternalValidate(object o, EngineIntrinsics engineIntrinsics) 
        { 
            Validate(o, engineIntrinsics);
        }
    }
}
