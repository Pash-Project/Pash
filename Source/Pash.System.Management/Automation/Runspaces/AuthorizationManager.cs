// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Host;

namespace System.Management.Automation.Runspaces
{
    public class AuthorizationManager
    {
        public AuthorizationManager(string shellId)
        {
        }

        protected internal virtual bool ShouldRun(CommandInfo commandInfo, CommandOrigin origin, PSHost host, out Exception reason)
        {
            throw new NotImplementedException();
        }
    }
}
