using System;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
    public sealed class ProviderIntrinsics
    {
        public ChildItemCmdletProviderIntrinsics ChildItem { get; private set; }

        public ContentCmdletProviderIntrinsics Content { get; private set; }

        public ItemCmdletProviderIntrinsics Item { get; private set; }

        public PropertyCmdletProviderIntrinsics Property { get; private set; }

        public SecurityDescriptorCmdletProviderIntrinsics SecurityDescriptor { get; private set; }

        // internals
        //internal ProviderIntrinsics(SessionStateInternal sessionState);

        private InternalCommand _cmdlet;
        
        internal ProviderIntrinsics(Cmdlet cmdlet)
        {
            if (cmdlet == null)
            {
                throw new NullReferenceException("Cmdlet can't be null.");
            }

            _cmdlet = cmdlet;
            ChildItem = new ChildItemCmdletProviderIntrinsics(cmdlet);
            Content = new ContentCmdletProviderIntrinsics(cmdlet);
            Item = new ItemCmdletProviderIntrinsics(cmdlet);
            Property = new PropertyCmdletProviderIntrinsics(cmdlet);
            SecurityDescriptor = new SecurityDescriptorCmdletProviderIntrinsics(cmdlet);
        }
    }
}
