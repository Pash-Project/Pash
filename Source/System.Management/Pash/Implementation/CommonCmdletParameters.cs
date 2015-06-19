using System;
using System.Management.Automation;

namespace Pash.Implementation
{
    class CommonCmdletParameters
    {
        [Alias("vb")]
        [Parameter]
        public SwitchParameter Verbose { get; set; }

        internal static CommandParameterDiscovery ParameterDiscovery = new CommandParameterDiscovery(typeof(CommonCmdletParameters));
    }
}
