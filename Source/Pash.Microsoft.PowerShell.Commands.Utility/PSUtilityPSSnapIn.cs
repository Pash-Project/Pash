using System;
using System.ComponentModel;
using System.Management.Automation;

namespace Microsoft.PowerShell
{
    [RunInstaller(true)]
    public sealed class PSUtilityPSSnapIn : PSSnapIn
    {
        public PSUtilityPSSnapIn()
        {
        }

        public override string Description
        {
            get
            {
                return "This PSSnapIn contains utility cmdlets used to manipulate data.";
            }
        }

        public override string DescriptionResource { get { throw  new NotImplementedException(); } }

        public override string Name
        {
            get
            {
                return "Microsoft.PowerShell.Utility";
            }
        }

        public override string Vendor { get { return "Pash"; } }

        public override string VendorResource { get { throw new NotImplementedException(); } }
    }
}