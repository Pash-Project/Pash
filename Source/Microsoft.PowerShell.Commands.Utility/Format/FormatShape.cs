using System;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace Microsoft.PowerShell.Commands.Utility
{
    public enum FormatShape
    {
        Table,
        List,
        Undefined
    }

    internal static class FormatShapeHelper
    {
        private const int MaxPropertiesInTable = 5;

        public static FormatShape SelectByData(PSObject data)
        {
            if (data.BaseObject == null) // if it's null we don't really care...
            {
                return FormatShape.List;
            }
            var defaultDisplayProperties = data.GetDefaultDisplayPropertySet();
            if (defaultDisplayProperties.Count <= MaxPropertiesInTable)
            {
                return FormatShape.Table;
            }
            return FormatShape.List;
        }
    }
}

