using System;
using System.Management.Automation.Host;
using MonoDevelop.Components;

namespace PashGui
{
    class Host : PSHost
    {
        readonly ConsoleView _consoleView;

        public Host(ConsoleView consoleView)
        {
            this._consoleView = consoleView;
            this._UserInterface = new UserInterface(this);
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

        public override System.Globalization.CultureInfo CurrentCulture
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Guid InstanceId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        class UserInterface : PSHostUserInterface
        {
            readonly Host _host;

            public UserInterface(Host host)
            {
                this._host = host;
            }

            public override PSHostRawUserInterface RawUI
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override System.Collections.Generic.Dictionary<string, System.Management.Automation.PSObject> Prompt(string caption, string message, System.Collections.ObjectModel.Collection<FieldDescription> descriptions)
            {
                throw new System.NotImplementedException();
            }

            public override int PromptForChoice(string caption, string message, System.Collections.ObjectModel.Collection<ChoiceDescription> choices, int defaultChoice)
            {
                throw new System.NotImplementedException();
            }

            public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
            {
                throw new System.NotImplementedException();
            }

            public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message, string userName, string targetName, System.Management.Automation.PSCredentialTypes allowedCredentialTypes, System.Management.Automation.PSCredentialUIOptions options)
            {
                throw new System.NotImplementedException();
            }

            public override string ReadLine()
            {
                throw new System.NotImplementedException();
            }

            public override System.Security.SecureString ReadLineAsSecureString()
            {
                throw new System.NotImplementedException();
            }

            public override void Write(string value)
            {
                throw new System.NotImplementedException();
            }

            public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
            {
                throw new System.NotImplementedException();
            }

            public override void WriteDebugLine(string message)
            {
                throw new System.NotImplementedException();
            }

            public override void WriteErrorLine(string value)
            {
                this._host._consoleView.WriteOutput("ERROR: ");
                this._host._consoleView.WriteOutput(value);
                this._host._consoleView.WriteOutput(Environment.NewLine);
            }

            public override void WriteLine(string value)
            {
                this._host._consoleView.WriteOutput(value);
                this._host._consoleView.WriteOutput(Environment.NewLine);
            }

            public override void WriteProgress(long sourceId, System.Management.Automation.ProgressRecord record)
            {
                throw new System.NotImplementedException();
            }

            public override void WriteVerboseLine(string message)
            {
                throw new System.NotImplementedException();
            }

            public override void WriteWarningLine(string message)
            {
                throw new System.NotImplementedException();
            }
        }

        readonly UserInterface _UserInterface;

        public override PSHostUserInterface UI
        {
            get
            {
                return this._UserInterface;
            }
        }

        public override Version Version
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}