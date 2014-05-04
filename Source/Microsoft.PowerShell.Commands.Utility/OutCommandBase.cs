using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands.Utility;
using Microsoft.SqlServer.Server;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
    public class OutCommandBase : PSCmdlet
    {
        internal OutputWriter OutputWriter { get; set; }


        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void ProcessRecord()
        {
            Collection<FormatData> data;
            if (InputObject.BaseObject is FormatData)
            {
                data = new Collection<FormatData>();
                data.Add((FormatData) InputObject.BaseObject);
            }
            else
            {
                data = FormatDefault(InputObject);
            }
            // although we might have multiple FormatData objects, they are all derived from a single data object
            // so all should be formatted in the same shape
            var processor = FormatProcessor.Get(OutputWriter, data[0].Shape);
            foreach (var curData in data)
            {
                processor.ProcessPayload(curData);
            }
        }

        private Collection<FormatData> FormatDefault(PSObject obj)
        {
            var data = new Collection<FormatData>();
            var pipeline = ExecutionContext.CurrentRunspace.CreateNestedPipeline("Format-Default", false);
            pipeline.Input.Write(obj, true); // TODO: sburnicki - check if we should enumerate this
            foreach (var res in pipeline.Invoke()) // error is hopefully propagated
            {
                var formatData = res.BaseObject as FormatData;
                if (formatData == null)
                {
                    var exc = new PSInvalidOperationException("Format-Default didn't return FormatData");
                    WriteError(exc.ErrorRecord);
                }
                data.Add(formatData);
            }
            return data;
        }
    }
}

