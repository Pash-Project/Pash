// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace System.Management.Automation.Provider
{
    public interface IPropertyCmdletProvider
    {
        void ClearProperty(Path path, Collection<string> propertyToClear);
        object ClearPropertyDynamicParameters(Path path, Collection<string> propertyToClear);
        void GetProperty(Path path, Collection<string> providerSpecificPickList);
        object GetPropertyDynamicParameters(Path path, Collection<string> providerSpecificPickList);
        void SetProperty(Path path, PSObject propertyValue);
        object SetPropertyDynamicParameters(Path path, PSObject propertyValue);
    }
}
