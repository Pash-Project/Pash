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
        private FormatManager _formatManager;
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
                if (_formatManager == null)
                {
                    _formatManager = new FormatManager(FormatShape.Undefined, ExecutionContext);
                }
                data = _formatManager.Process(InputObject);
            }
            // although we might have multiple FormatData objects, they are all derived from a single data object
            // so all should be formatted in the same shape
            var processor = FormatProcessor.Get(OutputWriter, data[0].Shape);
            foreach (var curData in data)
            {
                processor.ProcessPayload(curData);
            }
        }

        protected override void EndProcessing()
        {
            if (_formatManager != null)
            {
                var data = _formatManager.End();
                if (data.Count > 0)
                {
                    var processor = FormatProcessor.Get(OutputWriter, data[0].Shape);
                    foreach (var formatPayload in data)
                    {
                        processor.ProcessPayload(formatPayload);
                    }
                }
            }
            if (OutputWriter != null)
            {
                OutputWriter.Close();
            }
        }
    }
}

