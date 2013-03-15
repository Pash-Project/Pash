// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Security.AccessControl;

namespace System.Management.Automation.Provider
{
    public interface ISecurityDescriptorCmdletProvider
    {
        void GetSecurityDescriptor(Path path, AccessControlSections includeSections);
        ObjectSecurity NewSecurityDescriptorFromPath(Path path, AccessControlSections includeSections);
        ObjectSecurity NewSecurityDescriptorOfType(string type, AccessControlSections includeSections);
        void SetSecurityDescriptor(Path path, ObjectSecurity securityDescriptor);
    }
}
