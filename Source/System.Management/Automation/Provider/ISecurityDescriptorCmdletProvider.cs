// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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
