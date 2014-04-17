// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using Microsoft.PowerShell.Commands.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
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

            IEnumerable<PSObject> outputObjects = InputObjects;

            if (Unique.ToBool())
            {
                PropertyEqualityComparer comparer = new PropertyEqualityComparer(null);
                if (Property != null)
                    comparer.Properties = Property.Select(p => p.ToString()).ToList();

                PSObject prevObj = null;
                foreach (PSObject obj in InputObjects)
                {
                    if (!comparer.Equals(obj, prevObj))
                        WriteObject(obj);
                    prevObj = obj;
                }
            }
            else
            {
                foreach (PSObject obj in InputObjects)
                {
                    WriteObject(obj);
                }
            }
        }

        int Compare(PSObject x, PSObject y)
        {
            if (Descending.ToBool())
            {
                var temp = x;
                x = y;
                y = temp;
            }

            if (this.Property == null)
            {
                return LanguagePrimitives.Compare(x, y);
            }

            foreach (var property in this.Property)
            {
                var xPropertyValue = x.Properties[property.ToString()].Value;
                var yPropertyValue = y.Properties[property.ToString()].Value;

                var result = LanguagePrimitives.Compare(xPropertyValue, yPropertyValue);
                if (result != 0) return result;
            }

            return 0;
        }
    }
}
