// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
    /// <summary>
    ///  Base class for other classes which represent PSSnapins.
    /// </summary>
    public abstract class CustomPSSnapIn : PSSnapInInstaller
    {
	protected CustomPSSnapIn()
	{
		}

		public virtual Collection<CmdletConfigurationEntry> Cmdlets { get; private set; }

        public virtual Collection<FormatConfigurationEntry> Formats { get; private set; }

        public virtual Collection<ProviderConfigurationEntry> Providers { get; private set; }

        public virtual Collection<TypeConfigurationEntry> Types { get; private set; }
    }
}

