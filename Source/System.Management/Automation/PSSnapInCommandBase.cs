// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Collections.Generic;
using Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    public abstract class PSSnapInCommandBase : PSCmdlet, IDisposable
    {
        private const string NoSnapinsFoundFormat = "No SnapIns were found that match the pattern '{0}'.";

        private LocalRunspaceConfiguration LocalRunspaceConfiguration
        {
            get
            {
                return ExecutionContext.RunspaceConfiguration as LocalRunspaceConfiguration;
            }
        }

        protected PSSnapInCommandBase()
        {

        }

        protected Collection<PSSnapInInfo> GetRegisteredSnapIns(string pattern)
        {
            throw new NotImplementedException();
        }

        protected Collection<PSSnapInInfo> GetSnapIns(string pattern)
        {
            Collection<PSSnapInInfo> results;
            if (WildcardPattern.ContainsWildcardCharacters(pattern))
            {
                results = ExecutionContext.SessionStateGlobal.GetPSSnapIns(new WildcardPattern(pattern));
            }
            else //usual name
            {
                results = new Collection<PSSnapInInfo>();
                PSSnapInInfo info = ExecutionContext.SessionStateGlobal.GetPSSnapIn(pattern);
                if (info != null)
                {
                    results.Add(info);
                }
            }
            if (results.Count < 1)
            {
                throw new PSArgumentException(String.Format(NoSnapinsFoundFormat, pattern));
            }
            return results;
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }

}
