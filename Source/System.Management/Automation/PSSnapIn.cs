// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Collections.Generic;

namespace System.Management.Automation
{
    public abstract class PSSnapIn : PSSnapInInstaller
    {
        protected PSSnapIn()
        {

        }

        public virtual string[] Formats { get { return null; } }
        public virtual string[] Types { get { return null; } }
    }

}
