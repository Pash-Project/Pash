using System;
using TestPSSnapIn;

namespace ReferenceTests
{
    public class ModuleCommandTestBase : ReferenceTestBase
    {
        private string _assemblyTestModule;
        public string AssemblyTestModule
        {
            get
            {
                if (_assemblyTestModule == null)
                {
                    _assemblyTestModule = new UriBuilder(typeof(TestCommand).Assembly.CodeBase).Path;
                }
                return _assemblyTestModule;
            }
        }
    }
}

