// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;

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

            else throw new NotImplementedException(this.ToString());
        }
    }
}
