using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Security.AccessControl;

namespace System.Management.Automation
{
    public sealed class SecurityDescriptorCmdletProviderIntrinsics
    {
        private InternalCommand _cmdlet;
        internal SecurityDescriptorCmdletProviderIntrinsics(Cmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public Collection<PSObject> Get(string path, AccessControlSections includeSections) { throw new NotImplementedException(); }
        public ObjectSecurity NewFromPath(string path, AccessControlSections includeSections) { throw new NotImplementedException(); }
        public ObjectSecurity NewOfType(string providerId, string type, AccessControlSections includeSections) { throw new NotImplementedException(); }
        public Collection<PSObject> Set(string path, ObjectSecurity sd) { throw new NotImplementedException(); }

        // internals
        //internal void Get(string path, AccessControlSections includeSections, CmdletProviderContext context);
        //internal SecurityDescriptorCmdletProviderIntrinsics(SessionStateInternal sessionState);
        //internal void Set(string path, System.Security.AccessControl.ObjectSecurity sd, CmdletProviderContext context);
    }
}
