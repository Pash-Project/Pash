// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Host;
using System.Globalization;

namespace Microsoft.PowerShell
{
    internal class DefaultHost : PSHost
    {
        public override string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Version Version
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
        public override PSHostUserInterface UI
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override CultureInfo CurrentCulture
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override CultureInfo CurrentUICulture
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal DefaultHost(CultureInfo currentCulture, CultureInfo currentUICulture)
        {
            throw new NotImplementedException();
        }

        public override void SetShouldExit(int exitCode)
        {
            throw new NotImplementedException();
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
    }
}
