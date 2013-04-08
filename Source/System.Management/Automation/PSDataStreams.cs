// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation
{
    public sealed class PSDataStreams
    {
        private PowerShell powershell;

        /// <summary>
        /// TODO: .
        /// </summary>
        public PSDataCollection<ErrorRecord> Error
        {
            get
            {
                return new PSDataCollection<ErrorRecord>();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public PSDataCollection<ProgressRecord> Progress
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public PSDataCollection<VerboseRecord> Verbose
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public PSDataCollection<DebugRecord> Debug
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public PSDataCollection<WarningRecord> Warning
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal PSDataStreams(PowerShell pshell)
        {
            powershell = pshell;
        }

        public void ClearStreams()
        {
            this.Error.Clear();
            this.Progress.Clear();
            this.Verbose.Clear();
            this.Debug.Clear();
            this.Warning.Clear();
        }
    }
}
