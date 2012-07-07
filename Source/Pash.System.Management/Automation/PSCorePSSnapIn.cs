using System.ComponentModel;

namespace System.Management.Automation
{
    [RunInstaller(true)]
    public sealed class PSCorePSSnapIn : PSSnapIn
    {
        public PSCorePSSnapIn()
        {
        }

        // Properties
        public override string Description
        {
            get
            {
                return "This PSSnapIn contains MSH management cmdlets used to manage components affecting the MSH engine.";
            }
        }

        public override string DescriptionResource { get { throw new NotImplementedException(); } }

        public override string[] Formats
        {
            get
            {
                /* // TODO: implement formats
                return new string[] { 
                    "Certificate.format.ps1xml", 
                    "DotNetTypes.format.ps1xml", 
                    "FileSystem.format.ps1xml", 
                    "Help.format.ps1xml", 
                    "PowerShellCore.format.ps1xml", 
                    "PowerShellTrace.format.ps1xml", 
                    "Registry.format.ps1xml" 
                };*/
                throw new NotImplementedException();
            }
        }

        public override string Name
        {
            get
            {
                return "Microsoft.PowerShell.Core";
            }
        }

        public override string[] Types
        {
            get
            {
                /* // TODO: implement types extensions
                return new string[] { "types.ps1xml" };*/
                throw new NotImplementedException();
            }
        }

        public override string Vendor { get { return "Pash"; } }

        public override string VendorResource { get { throw new NotImplementedException(); } }
    }

 

}