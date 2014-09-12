using System;
using NUnit.Framework;
using System.Management.Automation.Runspaces;
using Pash.Implementation;
using System.Management.Automation;

namespace TestHost
{
    [TestFixture]
    public class TabExpansionProviderTests
    {
        private LocalRunspace _runspace;
        private TabExpansionProvider _tabExp;

        [TestFixtureSetUp]
        public void SetUpRunspace()
        {
            TestHost host = new TestHost(new TestHostUserInterface());
            // use public static property, so we can access e.g. the ExecutionContext after execution
            _runspace = RunspaceFactory.CreateRunspace(host) as LocalRunspace;
            _runspace.Open();
            _tabExp = new TabExpansionProvider(_runspace);
        }

        [TestFixtureTearDown]
        public void CloseRunspace()
        {
            _runspace.Close();
            _tabExp = null;
        }

        // escpaed spaces in cmds

        // files
            // hidden files
        // commands
        // finding cmdlet
        // parameters
        // functions
            // funs with scope
        // variables
            // vars with scope

        // 
    }
}

