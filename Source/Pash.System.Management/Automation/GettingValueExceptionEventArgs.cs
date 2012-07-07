using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public class GettingValueExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }
        public bool ShouldThrow { get; set; }
        public object ValueReplacement { get; set; }

        internal GettingValueExceptionEventArgs(System.Exception exception) { Exception = exception; }
    }
}
