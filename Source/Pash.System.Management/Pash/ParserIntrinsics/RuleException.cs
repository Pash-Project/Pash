using System;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
    [Serializable()]
    public class RuleException : System.Exception
    {

        public RuleException(string message)
            : base(message)
        {
        }

        public RuleException(string message,
                             Exception inner)
            : base(message, inner)
        {
        }

        protected RuleException(SerializationInfo info,
                                StreamingContext context)
            : base(info, context)
        {
        }

    }
}

