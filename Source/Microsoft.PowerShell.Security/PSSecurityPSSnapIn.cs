// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.ComponentModel;
using System.Management.Automation;

namespace Microsoft.PowerShell
{
    [RunInstaller(true)]
    public sealed class PSSecurityPSSnapin : PSSnapIn
    {
        public override string Description
        {
            get
            {
                return "This PSSnapIn contains security related cmdlets.";
            }
        }

        public override string DescriptionResource { get { throw new NotImplementedException(); } }

        public override string Name
        {
            get
            {
                return "Microsoft.PowerShell.Security";
            }
        }

        public override string Vendor { get { return "Pash"; } }

        public override string VendorResource { get { throw new NotImplementedException(); } }
    }
}

