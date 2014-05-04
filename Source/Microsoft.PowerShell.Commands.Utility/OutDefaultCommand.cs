// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation;
using Microsoft.PowerShell.Commands.Utility;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Out", "Default")]
    public class OutDefaultCommand : OutCommandBase
    {
        protected override void BeginProcessing()
        {
            OutputWriter = new ConsoleOutputWriter(Host);
        }

        /*

        private bool CheckWriteError(object obj)
        {
            if (!(obj is PSObject))
            {
                return false;
            }
            var writeToErrorProperty = ((PSObject) InputObject).Properties["writeToErrorStream"];
            return (writeToErrorProperty != null &&
                    writeToErrorProperty.Value is bool &&
                   (bool)writeToErrorProperty.Value);
        }

        protected override void ProcessRecord()
        {
            // TODO: output the data via OutHostCommand

            // TODO: should we print Null?
            if (InputObject == null || InputObject.ImmediateBaseObject == null)
                return;
            bool writeToError = CheckWriteError(InputObject);
            if (InputObject.BaseObject is Array)
            {
                foreach (var curObj in (Array) InputObject.BaseObject)
                {
                    if (writeToError || CheckWriteError(curObj))
                    {
                        Host.UI.WriteErrorLine(curObj.ToString());
                    }
                    else
                    {
                        Host.UI.WriteLine(curObj.ToString());
                    }
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
        */
    }
}
