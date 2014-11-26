using System;

namespace System.Management.Automation
{
    public abstract class LoopFlowException : FlowControlException
    {
        public string Label { get; private set; }

        internal LoopFlowException(string label)
        {
            Label = label;
        }
    }
}

