// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Collections;

namespace System.Management.Automation
{
    public abstract class PSInstaller // : Installer
    {
        protected PSInstaller()
        {

        }

        public virtual void Install(IDictionary stateSaver)
        {
            // Install(stateSaver);
        }

        public virtual void Rollback(IDictionary savedState)
        {
            // Rollback(savedState);
        }

        public virtual void Uninstall(IDictionary savedState)
        {
            // Uninstall(savedState);
        }
    }
}
