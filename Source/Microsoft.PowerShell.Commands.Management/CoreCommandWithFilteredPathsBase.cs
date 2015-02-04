using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public class CoreCommandWithFilteredPathsBase : CoreCommandWithPathsBase
    {
        [Parameter]
        public override string[] Exclude { get; set; }

        [Parameter]
        public override string Filter { get; set; }

        [Parameter]
        public override string[] Include { get; set; }
    }
}

