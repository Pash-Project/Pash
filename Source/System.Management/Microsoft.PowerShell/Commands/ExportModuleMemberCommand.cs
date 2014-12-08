using System;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Collections.Generic;
using Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    [CmdletAttribute("Export", "ModuleMember"
                     /*, HelpUri="http://go.microsoft.com/fwlink/?LinkID=141551" */)] 
    public class ExportModuleMemberCommand : PSCmdlet
    {
        [AllowEmptyCollection]
        [Parameter(ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, Position=0)]
        public string[] Function { get; set; }

        [ValidateNotNull]
        [Parameter(ValueFromPipelineByPropertyName=true)]
        public string[] Alias { get; set; }

        [AllowEmptyCollection]
        [Parameter(ValueFromPipelineByPropertyName=true)]
        public string[] Cmdlet { get; set; }

        [Parameter(ValueFromPipelineByPropertyName=true)]
        [ValidateNotNull]
        public string[] Variable { get; set; }

        private PSModuleInfo _affectedModule;

        protected override void BeginProcessing()
        {
            _affectedModule = ExecutionContext.SessionState.Module;
            if (_affectedModule == null)
            {
                throw new PSInvalidOperationException("You can execute Export-ModuleMember only inside a module",
                                                      "Modules_CanOnlyExecuteExportModuleMemberInsideAModule",
                                                      ErrorCategory.PermissionDenied);
            }
        }

        protected override void ProcessRecord()
        {
            _affectedModule.HasExplicitExports = true;
            var ss = ExecutionContext.SessionState;
            // resolve all function, variable, cmdlet, and alias patterns and add the matched members to the exports
            SelectMembersToExport(ss.Function.GetAllLocal(), Function, _affectedModule.ExportedFunctions);
            SelectMembersToExport(ss.PSVariable.GetAllLocal(), Variable, _affectedModule.ExportedVariables);
            SelectMembersToExport(ss.Alias.GetAllLocal(), Alias, _affectedModule.ExportedAliases);
            /* TODO: support for scoped cmdlets
            SelectMembersToExport(ss.Cmdlet.GetAllLocal(), Cmdlet, _affectedModule.ExportedCmdlets);
            */
        }

        private void SelectMembersToExport<T>(Dictionary<string, T> localItems, string[] patterns,
                                             Dictionary<string, T> exports)
        {
            var wildcards = StringArrayToWildcardArray(patterns);
            foreach (var pair in localItems)
            {
                if (MatchesAnyWildcard(pair.Key, wildcards))
                {
                    exports.Add(pair.Key, pair.Value);
                }
            }
        }

        private static WildcardPattern[] StringArrayToWildcardArray(string[] strs)
        {
            if (strs == null)
            {
                return new WildcardPattern[0];
            }
            return (from s in strs select new WildcardPattern(s, WildcardOptions.IgnoreCase)).ToArray();
        }

        private static bool MatchesAnyWildcard(string item, WildcardPattern[] wildcards)
        {
            foreach (var wc in wildcards)
            {
                if (wc.IsMatch(item))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

