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
            Assert.AreEqual(3, Execute("1 + 2").Single().ImmediateBaseObject);
        }

        [Test]
        public void ConcatStringInteger()
        {
            Assert.AreEqual("xxx1", Execute("'xxx' + 1").Single().ImmediateBaseObject);
        }

        [Test]
        public void VerbatimString()
        {
            Assert.AreEqual("xxx", Execute("'xxx'").Single().ImmediateBaseObject);
        }

        private static Collection<PSObject> Execute(string statement)
        {
            var myHost = new TestHost();
            var myRunSpace = RunspaceFactory.CreateRunspace(myHost);
            myRunSpace.Open();

            using (var currentPipeline = myRunSpace.CreatePipeline())
            {
                currentPipeline.Commands.Add(statement);
                return currentPipeline.Invoke();
            }
        }
    }

    class TestHost : PSHost
    {
        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { throw new NotImplementedException(); }
        }

        public override Guid InstanceId
        {
            get { throw new NotImplementedException(); }
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override PSHostUserInterface UI
        {
            get { throw new NotImplementedException(); }
        }

        public override Version Version
        {
            get { throw new NotImplementedException(); }
        }

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void NotifyBeginApplication()
        {
            throw new NotImplementedException();
        }

        public override void NotifyEndApplication()
        {
            throw new NotImplementedException();
        }

        public override void SetShouldExit(int exitCode)
        {
            throw new NotImplementedException();
        }
    }

}
