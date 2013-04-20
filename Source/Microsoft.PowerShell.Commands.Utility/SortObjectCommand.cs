// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using System.Reflection;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Sort", "Object")]
    public sealed class SortObjectCommand : OrderObjectBase
    {
        [Parameter]
        public SwitchParameter Descending { get; set; }

        [Parameter]
        public SwitchParameter Unique { get; set; }

        protected override void EndProcessing()
        {
            InputObjects.Sort(Compare);

            foreach (PSObject obj in InputObjects)
            {
                WriteObject(obj);
            }
        }

        int Compare(PSObject x, PSObject y)
        {
            if (this.Property == null)
            {
                return LanguagePrimitives.Compare(x, y);
            }

            foreach (var property in this.Property)
            {
                var xPropertyValue = x.BaseObject.GetType().GetProperty(property.ToString(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase).GetValue(x.BaseObject, null);
                var yPropertyValue = y.BaseObject.GetType().GetProperty(property.ToString(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase).GetValue(y.BaseObject, null);

                var result = LanguagePrimitives.Compare(xPropertyValue, yPropertyValue);
                if (result != 0) return result;
            }

            return 0;
        }
    }
}
