using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
    public class OrderObjectBase : ObjectCmdletBase
    {
        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Parameter(Position = 0)]
        public object[] Property { get; set; }

        // TODO: implement order
        internal SwitchParameter DescendingOrder { get; set; }
        internal List<PSObject> InputObjects { get; private set; }

        public OrderObjectBase()
        {
            InputObject = AutomationNull.Value;
            InputObjects = new List<PSObject>();
        }

        protected override void ProcessRecord()
        {
            if ((InputObject != null) && (InputObject != AutomationNull.Value))
            {
                InputObjects.Add(InputObject);
            }
        }

    }
}