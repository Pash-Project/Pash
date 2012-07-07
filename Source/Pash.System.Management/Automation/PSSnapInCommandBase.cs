using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class PSSnapInCommandBase : Cmdlet, IDisposable
    {
        protected PSSnapInCommandBase()
        {
            
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }

}