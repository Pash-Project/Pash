// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Collections.Generic;

namespace System.Management.Automation
{
    public abstract class PSSnapInInstaller : PSInstaller
    {
        protected PSSnapInInstaller()
        {

        }

        public abstract string Description { get; }
        public virtual string DescriptionResource { get { return null; } }
        public abstract string Name { get; }
        public abstract string Vendor { get; }
        public virtual string VendorResource { get { return null; } }
    }
}
