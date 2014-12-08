using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace System.Management.Automation
{
    // TODO: remove DefaultParameterSetName attribute (see issue #320)
    [Cmdlet(VerbsCommon.Remove, "Module", SupportsShouldProcess=true, DefaultParameterSetName = "name"
            /*, HelpUri="http://go.microsoft.com/fwlink/?LinkID=141556"*/)]
    public class RemoveModuleCommand : ModuleCmdletBase
    {
        [Parameter(Mandatory=true, ParameterSetName="ModuleInfo", ValueFromPipeline=true, Position=0)] 
        public PSModuleInfo[] ModuleInfo { get; set; }

        [Parameter(Mandatory=true, ParameterSetName="name", ValueFromPipeline=true, Position=0)] 
        public string[] Name { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            if (Name != null && Name.Length > 0)
            {
                foreach (var curName in Name)
                {
                    RemoveModules(GetLoadedModulesByWildcard(curName));
                }
            }
            else if (ModuleInfo != null && ModuleInfo.Length > 0)
            {
                RemoveModules(ModuleInfo);
            }
        }

        private IEnumerable<PSModuleInfo> GetLoadedModulesByWildcard(string wildcardStr)
        {
            var wildcard = new WildcardPattern(wildcardStr, WildcardOptions.IgnoreCase);
            var modules = from modPair in ExecutionContext.SessionState.LoadedModules.GetAll()
                where wildcard.IsMatch(modPair.Value.Name) select modPair.Value;
            return modules;
        }

        private void RemoveModules(IEnumerable<PSModuleInfo> modules)
        {
            foreach (var mod in modules)
            {
                // TODO: some security checks. E.g. if it's ReadOnly access mode. Then we need to use ShouldContinue
                // and the Force parameter
                ExecutionContext.SessionState.LoadedModules.Remove(mod);
            }
        }
    }
}

