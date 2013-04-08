﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class PSSnapInCommandBase : Cmdlet, IDisposable
    {
        protected PSSnapInCommandBase()
        {

        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }

}
