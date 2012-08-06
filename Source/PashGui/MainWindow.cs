using System;
using Gtk;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Host;
using PashGui;

public partial class MainWindow: Gtk.Window
{
    readonly private Runspace runspace;
    readonly private Host host;

    public MainWindow(): base (Gtk.WindowType.Toplevel)
    {
        Build();

        this.consoleview1.ConsoleInput += this.OnConsoleview1ConsoleInput;

        this.host = new Host(this.consoleview1);
        this.runspace = RunspaceFactory.CreateRunspace(this.host);
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
        using (Pipeline currentPipeline = BuildExecutePipeline(command))
        {
            currentPipeline.Invoke();
        }
    }

    void ExecutePrompt()
    {
        this.consoleview1.PromptString = "PASH> ";
        this.consoleview1.Prompt(false);
    }

    protected void OnConsoleview1ConsoleInput(object sender, MonoDevelop.Components.ConsoleInputEventArgs e)
    {
        string command = e.Text;

        ExecuteCommand(command);
        ExecutePrompt();
    }

    public Pipeline BuildExecutePipeline(string command)
    {
        Pipeline currentPipeline = this.runspace.CreatePipeline();

        currentPipeline.Commands.Add(command);

        // Now add the default outputter to the end of the pipe and indicate
        // that it should handle both output and errors from the previous
        // commands. This will result in the output being written using the PSHost
        // and PSHostUserInterface classes instead of returning objects to the hosting
        // application.
        currentPipeline.Commands.Add("out-default");
        currentPipeline.Commands [0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

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
        public void BuildExecutePipelineTest()
        {
            using (Pipeline pipeline = new MainWindow().BuildExecutePipeline("get-childitem"))
            {
                Assert.AreEqual(2, pipeline.Commands.Count);
                Assert.AreNotEqual("get-childitem", pipeline.Commands [0]);
                Assert.AreNotEqual("out-default", pipeline.Commands [1]);
            }
        }
    }
}