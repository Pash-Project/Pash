using System;
using System.ComponentModel;
using System.Management.Automation;

namespace Microsoft.Commands.Management
{
    [RunInstaller(true)]
    public sealed class PSManagementPSSnapIn : PSSnapIn
    {
        public PSManagementPSSnapIn()
        {
        }

        public override string Description
        {
            get
            {
                return "This PSSnapIn contains general management cmdlets used to manage Windows components.";
            }
        }
 
        public override string DescriptionResource { get { throw new NotImplementedException(); } }

        public override string Name
        {
            get
            {
                return "Microsoft.PowerShell.Management";
            }
        }

        public override string Vendor { get { return "Pash"; } }

        public override string VendorResource { get { throw new NotImplementedException(); } }
    }
}