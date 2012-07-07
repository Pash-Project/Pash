using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
    public abstract class FrontEndCommandBase : PSCmdlet, IDisposable
    {
        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected FrontEndCommandBase()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}