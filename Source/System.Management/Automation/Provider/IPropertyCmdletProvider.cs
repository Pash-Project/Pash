// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Provider
{
    public interface IPropertyCmdletProvider
    {
        void ClearProperty(string path, Collection<string> propertyToClear);
        object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear);
        void GetProperty(string path, Collection<string> providerSpecificPickList);
        object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList);
        void SetProperty(string path, PSObject propertyValue);
        object SetPropertyDynamicParameters(string path, PSObject propertyValue);
    }
}
