using System;
using Gtk;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Host;
using PashGui;

public partial class MainWindow: Gtk.Window
{
    readonly Model Model;

    Runspace runspace { get { return this.Model.runspace; } }

    public MainWindow(): base (Gtk.WindowType.Toplevel)
    {
        Build();

        this.consoleview1.ConsoleInput += this.OnConsoleview1ConsoleInput;

        Runspace runspace = RunspaceFactory.CreateRunspace(new Host(this.consoleview1));
        this.Model = new Model(runspace);

        this.runspace.Open();

        ExecutePrompt();
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    void ExecuteCommand(string command)
    {
        using (Pipeline currentPipeline = BuildExecutePipeline(command, true))
        {
            currentPipeline.Invoke();
        }
    }

    void ExecutePrompt()
    {
        this.consoleview1.PromptString = "PASH> ";

        using (var pipeline = BuildExecutePipeline("prompt", false))
        {
            var result = string.Join("", pipeline.Invoke());
            if (!string.IsNullOrEmpty(result))
            {
                this.consoleview1.PromptString = result;
            }
        }

        this.consoleview1.Prompt(false);
    }

    protected void OnConsoleview1ConsoleInput(object sender, MonoDevelop.Components.ConsoleInputEventArgs e)
    {
        string command = e.Text;

        ExecuteCommand(command);
        ExecutePrompt();
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
    public class MainWindowTests
    {
        [Test]
        public void ATest()
        {
            new MainWindow();
        }

        [Test]
        public void BuildExecutePipelineTrueTest()
        {
            using (Pipeline pipeline = new MainWindow().BuildExecutePipeline("get-childitem", true))
            {
                Assert.AreEqual(2, pipeline.Commands.Count);

                StringAssert.AreEqualIgnoringCase("get-childitem", pipeline.Commands [0].CommandText);
                StringAssert.AreEqualIgnoringCase("out-default", pipeline.Commands [1].CommandText);
            }
        }

        [Test]
        public void BuildExecutePipelineFalseTest()
        {
            using (Pipeline pipeline = new MainWindow().BuildExecutePipeline("prompt", false))
            {
                Assert.AreEqual(1, pipeline.Commands.Count);

                StringAssert.AreEqualIgnoringCase("prompt", pipeline.Commands [0].CommandText);
            }
        }
    }
}