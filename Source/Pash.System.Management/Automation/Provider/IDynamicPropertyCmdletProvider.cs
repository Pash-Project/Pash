using System;

namespace System.Management.Automation.Provider
{
    public interface IDynamicPropertyCmdletProvider : IPropertyCmdletProvider
    {
        void CopyProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty);
        object CopyPropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty);
        void MoveProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty);
        object MovePropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty);
        void NewProperty(string path, string propertyName, string propertyTypeName, object value);
        object NewPropertyDynamicParameters(string path, string propertyName, string propertyTypeName, object value);
        void RemoveProperty(string path, string propertyName);
        object RemovePropertyDynamicParameters(string path, string propertyName);
        void RenameProperty(string path, string sourceProperty, string destinationProperty);
        object RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty);
    }
}
