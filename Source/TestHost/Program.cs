using System;
using System.Linq;
using System.Management.Automation.Runspaces;
using Pash.Implementation;
using System.Management.Automation.Host;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Management.Automation;
using NUnit.Framework;

namespace TestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            // I'd like to see this run NUnit, so we can debug these tests easilly.
            throw new NotImplementedException();
        }

        [Test]
        public void AddIntegers()
        {
            Assert.AreEqual("3\r\n", Execute("1 + 2"));
        }

        [Test]
        public void ConcatStringInteger()
        {
            Assert.AreEqual("xxx1\r\n", Execute("'xxx' + 1"));
        }

        [Test]
        public void VerbatimString()
        {
            Assert.AreEqual("xxx\r\n", Execute("'xxx'"));
        }

        [Test]
        public void WriteOutputString()
        {
            Assert.AreEqual("xxx\r\n", Execute("Write-Output 'xxx'"));
        }

        [Test]
        public void WriteHost()
        {
            Assert.AreEqual("xxx\r\n", Execute("Write-Host 'xxx'"));
        }

        string Execute(string statement)
        {
            TestHostUserInterface ui = new TestHostUserInterface();

            TestHost host = new TestHost(ui);
            var myRunSpace = RunspaceFactory.CreateRunspace(host);
            myRunSpace.Open();

            using (var currentPipeline = myRunSpace.CreatePipeline())
            {
                currentPipeline.Commands.Add(statement);
                currentPipeline.Commands.Add("Out-Def");
                currentPipeline.Invoke();
            }

            return ui.Log.ToString();
        }
    }
}
