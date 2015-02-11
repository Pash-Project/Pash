// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [Cmdlet("Rename", "Item", DefaultParameterSetName="ByPath"
        /*, SupportsTransactions=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113382"*/)] 
    public class RenameItemCommand : CoreCommandWithCredentialsBase
    {
        protected string InternalPath;

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string NewName { get; set; }

        [Parameter]
        public override SwitchParameter Force { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }


        [Parameter(Position=0, ParameterSetName="ByPath", Mandatory=true, ValueFromPipeline=true,
            ValueFromPipelineByPropertyName=true)]
        public string Path
        {
            get { return InternalPath; }
            set { InternalPath = value; }
        }

        [Parameter(ParameterSetName="ByLiteralPath", Mandatory=true, ValueFromPipeline=false,
            ValueFromPipelineByPropertyName=true)]
        [Alias("PSPath")]
        public string LiteralPath
        {
            get { return InternalPath; }
            set
            {
                AvoidWildcardExpansion = true;
                InternalPath = value;
            }
        }

        // TODO: support for #DynamicParameters (calling the providers appropriate method)

        protected override void ProcessRecord()
        {
            var runtime = ProviderRuntime;
            runtime.PassThru = PassThru.IsPresent;
            InvokeProvider.Item.Rename(InternalPath, NewName, runtime);
        }
    }



}
