// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace TestHost
{
    class TestHost : PSHost
    {
        readonly PSHostUserInterface _ui = new TestHostUserInterface();

        public static InitialSessionState InitialSessionState { get; set; }

        public static string Execute(params string[] statements)
        {
            return Execute(false, null, statements);
        }

        public static string Execute(bool logErrors, params string[] statements)
        {
            if (logErrors)
            {
                return ExecuteWithZeroErrors(statements);
            }
            return Execute(statements);
        }

        public static string ExecuteWithZeroErrors(params string[] statements)
        {
            //Execute
            return Execute(true, null, statements);
        }

        private static string Execute(bool logErrors, Action<string> onErrorHandler, params string[] statements)
        {
            TestHostUserInterface ui = new TestHostUserInterface();

            if (logErrors)
            {
                ui.OnWriteErrorLineString = onErrorHandler ?? (s => ui.Log.Append(s));
            }

            TestHost host = new TestHost(ui);
            var myRunSpace = CreateRunspace(host);
            myRunSpace.Open();

            foreach (var statement in statements)
            {
                using (var currentPipeline = myRunSpace.CreatePipeline())
                {
                    currentPipeline.Commands.AddScript(statement, false);
                    currentPipeline.Commands.Add("Out-Default");
                    currentPipeline.Invoke();
                    // pipeline might failed, write errors to ui
                    if (currentPipeline.PipelineStateInfo.State.Equals(PipelineState.Failed))
                    {
                        foreach (var error in currentPipeline.Error.ReadToEnd())
                        {
                            ui.WriteErrorLine(error.ToString());
                        }
                    }
                }
            }

            return ui.Log.ToString();
        }

        public static Runspace CreateRunspace(PSHost host)
        {
            if (InitialSessionState != null)
            {
                return RunspaceFactory.CreateRunspace(host, InitialSessionState);
            }
            return RunspaceFactory.CreateRunspace(host);
        }

        public TestHost(TestHostUserInterface ui)
        {
            // TODO: Complete member initialization
            this._ui = ui;
        }

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
            get { return "TestHost"; }
        }

        public override PSHostUserInterface UI
        {
            get
            {
                return this._ui;
            }
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
