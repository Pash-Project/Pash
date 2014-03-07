using System;
using System.Management.Automation;

namespace TestHost
{
    public class TestException : Exception
    {
        public TestException(string message)
            : base(message)
        {
        }

        public ErrorRecord ErrorRecord
        {
            get
            {
                return new ErrorRecord(this, "Test", ErrorCategory.NotSpecified, null);
            }
        }
    }
}

