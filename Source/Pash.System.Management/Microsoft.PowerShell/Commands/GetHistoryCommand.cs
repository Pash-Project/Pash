using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Get", "History")]
    public class GetHistoryCommand : PSCmdlet
    {
        [ValidateRange(0, 0x7fff), Parameter(Position = 1)]
        public int Count { get; set; }

        [ValidateRange(1L, 0x7fffffffffffffffL), Parameter(Position = 0, ValueFromPipeline = true)]
        public long[] Id { get; set; }

        public GetHistoryCommand()
        {
            
        }

        protected override void ProcessRecord()
        {
            throw new NotImplementedException();
        }
    }
}