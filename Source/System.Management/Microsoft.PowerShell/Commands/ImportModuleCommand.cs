using System;
using System.Collections;
using System.Collections.Generic;
using Pash.Implementation;

namespace System.Management.Automation
{
    [CmdletAttribute(VerbsData.Import, "Module", DefaultParameterSetName="Name")] 
    public sealed class ImportModuleCommand : ModuleCmdletBase
    {
        [ParameterAttribute(ParameterSetName="Name", Mandatory=true, ValueFromPipeline=true, Position=0)] 
        //[ParameterAttribute(ParameterSetName="PSSession", Mandatory=true, ValueFromPipeline=true, Position=0)] 
        //[ParameterAttribute(ParameterSetName="CimSession", Mandatory=true, ValueFromPipeline=true, Position=0)] 
        public string[] Name { get; set; }

        [Parameter]
        [ValidateNotNullAttribute] 
        public string[] Alias { get; set; }

        [Parameter]
        [ValidateNotNullAttribute] 
        public string[] Cmdlet { get; set; }

        [Parameter]
        [ValidateNotNullAttribute] 
        public string[] Function { get; set; }

        [Parameter]
        [ValidateNotNullAttribute] 
        public string[] Variable { get; set; }

        [Parameter]
        public SwitchParameter Global { get; set; }

        [Parameter] 
        [ValidateSetAttribute("Global", "Local", IgnoreCase = true)] 
        public string Scope { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        private bool _loadToGlobalScope;

        protected override void BeginProcessing()
        {
            if (Global.IsPresent && !String.IsNullOrEmpty(Scope))
            {
                throw new MethodInvocationException("You cannot specify both the Global and the Scope parameter!");
            }
            // tests show somehow that we load the module to global scope by default rather than to local scope
            // as documentation says?
            _loadToGlobalScope = !String.Equals(Scope, "Local", StringComparison.InvariantCultureIgnoreCase);
        }

        protected override void ProcessRecord()
        {
            if (!ParameterSetName.Equals("Name"))
            {
                throw new NotImplementedException("Currently you can only import modules by name!");
            }
            var moduleLoader = new ModuleLoader(ExecutionContext);
            foreach (var modName in Name)
            {
                PSModuleInfo moduleInfo = moduleLoader.LoadModuleByName(modName, _loadToGlobalScope);
                if (PassThru.IsPresent)
                {
                    WriteObject(moduleInfo);
                }
            }
        }
    }
}

