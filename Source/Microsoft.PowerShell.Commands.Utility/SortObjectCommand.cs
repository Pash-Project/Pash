// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

        public SortObjectCommand()
        {
        }

        protected override void EndProcessing()
        {
            InputObjects.Sort();

            foreach (PSObject obj in InputObjects)
            {
                WriteObject(obj);
            }
        }
    }
}
