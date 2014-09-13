using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using NUnit.Framework;

namespace ReferenceTests.API
{
    [TestFixture]
    class DefaultScriptScopeTests
    {
        [Test]
        public void CommandCollectionAddScriptDefaultLocalScopeIsFalse()
        {
            var state = InitialSessionState.CreateDefault();
            Runspace runspace = RunspaceFactory.CreateRunspace(state);
            runspace.Open();
            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript("$a = 12");
            pipeline.Invoke();

            pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript("$a");
            List<string> objects = pipeline.Invoke().AsEnumerable().Select(obj => obj.ToString()).ToList();

            Assert.AreEqual("12", objects[0]);
        }
    }
}
