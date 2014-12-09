using System;
using TestPSSnapIn;

namespace ReferenceTests
{
    public class ModuleCommandTestBase : ReferenceTestBase
    {
        private string _binaryTestModule;
        public string BinaryTestModule
        {
            get
            {
                if (_binaryTestModule == null)
                {
                    _binaryTestModule = new UriBuilder(typeof(TestCommand).Assembly.CodeBase).Path;
                }
                return _binaryTestModule;
            }
        }
    }
}

