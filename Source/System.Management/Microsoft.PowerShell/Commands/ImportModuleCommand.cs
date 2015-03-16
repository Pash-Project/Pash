// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using Pash.Implementation;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    [CmdletAttribute(VerbsData.Import, "Module", DefaultParameterSetName="Name")]
    [OutputType(typeof(PSModuleInfo))]
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
        private ModuleIntrinsics.ModuleImportScopes _importScope;
        private ModuleLoader _moduleLoader;

        protected override void BeginProcessing()
        {
            _moduleLoader = new ModuleLoader(ExecutionContext);
            // evaluate meaning of scope parameters:
            // -Global or -Scope global: Import module and items to global session state
            // no options: import module to current modules session state table, import items to module scope
            // -scope local: import module to current modules session state table, import items to *local scope*
            if (Global.IsPresent && !String.IsNullOrEmpty(Scope))
            {
                throw new MethodInvocationException("You cannot specify both the Global and the Scope parameter!");
            }

            _loadToGlobalScope = Global.IsPresent || String.Equals(Scope, "Global", StringComparison.InvariantCultureIgnoreCase);
            _importScope = _loadToGlobalScope ? ModuleIntrinsics.ModuleImportScopes.Global
                                              : ModuleIntrinsics.ModuleImportScopes.Module;
            _importScope = String.Equals(Scope, "Local", StringComparison.InvariantCultureIgnoreCase) ? 
                                ModuleIntrinsics.ModuleImportScopes.Local : _importScope;

        }

        protected override void ProcessRecord()
        {
            if (!ParameterSetName.Equals("Name"))
            {
                throw new NotImplementedException("Currently you can only import modules by name!");
            }
            foreach (var modName in Name)
            {
                var module = LoadModule(modName);
                if (module == null)
                {
                    continue;
                }
                // (re-) import the members to specified scope
                SessionState.LoadedModules.ImportMembers(module, _importScope);

                if (PassThru.IsPresent)
                {
                    WriteObject(module);
                }
            }
        }

        private PSModuleInfo LoadModule(string name)
        {
            PSModuleInfo moduleInfo = null;
            try
            {
                // last arg: importMembers is false, because we need to (re-)import the members explicitly
                // to a maybe different scope
                moduleInfo = _moduleLoader.LoadModuleByName(name, _loadToGlobalScope, false);
            }
            catch (PSArgumentException e)
            {
                WriteError(e.ErrorRecord);
            }
            catch (PSInvalidOperationException e)
            {
                if (e.Terminating)
                {
                    ThrowTerminatingError(e.ErrorRecord);
                }
                WriteError(e.ErrorRecord);
            }

            return moduleInfo;
        }
    }
}

