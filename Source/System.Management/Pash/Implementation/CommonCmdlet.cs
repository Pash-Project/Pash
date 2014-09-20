using System;
using System.Management.Automation;

namespace Pash.Implementation
{
    [Cmdlet("Common", "Parameters", DefaultParameterSetName = ParameterAttribute.AllParameterSets)]
    class CommonParametersCmdlet
    {
        [Alias("vb")]
        [Parameter]
        public SwitchParameter Verbose { get; set; }
    }
}
