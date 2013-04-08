// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation.Host;
using System.Runtime.InteropServices;

namespace System.Management.Automation
{
    /// <summary>
    /// Allows for the blocking of cmdlets from running (using an inhereted ShouldRun method).
    /// </summary>
    public class AuthorizationManager
    {
        private string _shellId;

        public AuthorizationManager(string shellId)
        {
            _shellId = shellId;
        }

        protected virtual bool ShouldRun(CommandInfo commandInfo, CommandOrigin origin, PSHost host, out Exception reason)
        {
            reason = null;
            return true;
        }

    }
}

