using System;

namespace System.Management.Automation
{
    public class ContinueException : LoopFlowException
    {
        public ContinueException(string label) : base(label)
        {
        }
    }
}

