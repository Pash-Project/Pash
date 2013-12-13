using System;
using System.Management.Automation;

namespace TestPSSnapIn
{
    public class PashTestSnapIn : PSSnapIn
    {
		public override string Description
		{
			get { return "Snapin to test Pash's snapin support."; }
		}

		public override string Name
		{
			get { return "PashTestSnapIn"; }
		}

		public override string Vendor
		{
			get { return "Pash"; }
		}
    }
}

