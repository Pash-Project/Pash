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
        using (Pipeline currentPipeline = this.Model.BuildExecutePipeline(command, true))
        {
            currentPipeline.Invoke();
        }
    }

    void ExecutePrompt()
    {
        this.consoleview1.PromptString = "PASH> ";

        using (var pipeline = this.Model.BuildExecutePipeline("prompt", false))
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

}
