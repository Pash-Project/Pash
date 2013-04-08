// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

        private Cmdlet _cmdlet;

        internal ProviderIntrinsics(Cmdlet cmdlet)
        {
            if (cmdlet == null)
            {
                throw new NullReferenceException("Cmdlet can't be null.");
            }

            _cmdlet = cmdlet;
            ChildItem = new ChildItemCmdletProviderIntrinsics(_cmdlet);
            Content = new ContentCmdletProviderIntrinsics(_cmdlet);
            Item = new ItemCmdletProviderIntrinsics(_cmdlet);
            Property = new PropertyCmdletProviderIntrinsics(_cmdlet);
            SecurityDescriptor = new SecurityDescriptorCmdletProviderIntrinsics(_cmdlet);
        }
    }
}
