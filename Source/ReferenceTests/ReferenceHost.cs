using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceTests
{
    internal class ReferenceHost : TestHost.TestHost
    {
        public ReferenceHost() : base(new TestHost.TestHostUserInterface())
        {
        }
    }
}
