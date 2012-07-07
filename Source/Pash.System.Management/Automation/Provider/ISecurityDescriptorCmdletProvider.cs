using System;
using System.Security.AccessControl;

namespace System.Management.Automation.Provider
{
    public interface ISecurityDescriptorCmdletProvider
    {
        void GetSecurityDescriptor(string path, AccessControlSections includeSections);
        ObjectSecurity NewSecurityDescriptorFromPath(string path, AccessControlSections includeSections);
        ObjectSecurity NewSecurityDescriptorOfType(string type, AccessControlSections includeSections);
        void SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor);
    }
}
