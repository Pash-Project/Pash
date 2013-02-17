// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Automation.Runspaces
{
    public abstract class InitialSessionStateEntry
    {
        string name;
        PSSnapInInfo psSnapIn;
        PSModuleInfo module;

        protected InitialSessionStateEntry(string name)
        {
            this.name = name;
            this.psSnapIn = null;
            this.module = null;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            internal set
            {
                this.name = value;
            }
        }

        public PSSnapInInfo PSSnapIn
        {
            get
            {
                return this.psSnapIn;
            }
        }

        public PSModuleInfo Module
        {
            get
            {
                return this.module;
            }
        }

        public abstract InitialSessionStateEntry Clone();
    }
}
