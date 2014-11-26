using System;

namespace System.Management.Automation
{
    public class BreakException : LoopFlowException
    {
        public BreakException(string label) : base(label)
        {
        }
    }
}

