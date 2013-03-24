﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation.Host;
using NUnit.Framework;

namespace TestHost
{
    class TestHostUserInterface : PSHostUserInterface
    {
        public Action<string> OnWriteErrorLineString = delegate(string s) { Assert.Fail(s); };

        public override PSHostRawUserInterface RawUI
        {
            get { throw new NotImplementedException(); }
        }

        public override Dictionary<string, System.Management.Automation.PSObject> Prompt(string caption, string message, System.Collections.ObjectModel.Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, System.Collections.ObjectModel.Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new NotImplementedException();
        }

        public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException();
        }

        public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message, string userName, string targetName, System.Management.Automation.PSCredentialTypes allowedCredentialTypes, System.Management.Automation.PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override System.Security.SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }

        public override void Write(string value)
        {
            Log.Append(value);
        }

        public StringBuilder Log = new StringBuilder();

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            Log.Append(value);
        }

        public override void WriteDebugLine(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteErrorLine(string value)
        {
            this.OnWriteErrorLineString(value);
        }

        public override void WriteLine(string value)
        {
            this.Log.AppendLine(value);
        }

        public override void WriteProgress(long sourceId, System.Management.Automation.ProgressRecord record)
        {
            throw new NotImplementedException();
        }

        public override void WriteVerboseLine(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteWarningLine(string message)
        {
            throw new NotImplementedException();
        }
    }
}
