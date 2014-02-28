// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Out", "Default")]
    public class OutDefaultCommand : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        protected override void BeginProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            // TODO: output the data via OutHostCommand

            // TODO: should we print Null?
            if (InputObject == null || InputObject.ImmediateBaseObject == null)
                return;
            var writeToErrorProperty = InputObject.Properties["writeToErrorStream"];
            bool writeToError = (writeToErrorProperty != null && writeToErrorProperty.Value is bool && (bool)writeToErrorProperty.Value);
            if (InputObject.BaseObject is Array)
            {
                if (writeToError)
                {
                    Host.UI.WriteErrorLine(string.Join(Environment.NewLine, (object[])InputObject.BaseObject));
                }
                else
                {
                    Host.UI.WriteLine(string.Join(Environment.NewLine, (object[])InputObject.BaseObject));
                }
            }
            else
            {
                if (writeToError)
                {
                    Host.UI.WriteErrorLine(InputObject.ToString());
                }
                else
                {
                    Host.UI.WriteLine(InputObject.ToString());
                }
            }
        }
    }
}
