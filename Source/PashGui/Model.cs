using System;
using System.Management.Automation.Runspaces;

namespace PashGui
{
    class Model
    {
        readonly public Runspace runspace;

        public Model(Runspace runspace)
        {
            this.runspace = runspace;
        }

        public Pipeline BuildExecutePipeline(string command, bool resultsToOutDefault)
        {
            Pipeline currentPipeline = this.runspace.CreatePipeline();

            currentPipeline.Commands.Add(command);

            if (resultsToOutDefault)
            {
                // Now add the default outputter to the end of the pipe and indicate
                // that it should handle both output and errors from the previous
                // commands. This will result in the output being written using the PSHost
                // and PSHostUserInterface classes instead of returning objects to the hosting
                // application.
                currentPipeline.Commands.Add("out-default");
            }

            return currentPipeline;
        }
    }

    namespace Tests
    {
    using NUnit.Framework;

        [TestFixture]
        public class ModelTests
        {
            [Test]
            public void BuildExecutePipelineTrueTest()
            {
                Runspace runspace = RunspaceFactory.CreateRunspace();
                using (Pipeline pipeline = new Model(runspace).BuildExecutePipeline("get-childitem", true))
                {
                    Assert.AreEqual(2, pipeline.Commands.Count);

                    StringAssert.AreEqualIgnoringCase("get-childitem", pipeline.Commands [0].CommandText);
                    StringAssert.AreEqualIgnoringCase("out-default", pipeline.Commands [1].CommandText);
                }
            }

            [Test]
            public void BuildExecutePipelineFalseTest()
            {
                Runspace runspace = RunspaceFactory.CreateRunspace();
                using (Pipeline pipeline = new Model(runspace).BuildExecutePipeline("prompt", false))
                {
                    Assert.AreEqual(1, pipeline.Commands.Count);

                    StringAssert.AreEqualIgnoringCase("prompt", pipeline.Commands [0].CommandText);
                }
            }
        }
    }
}

