using System;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Collections.Generic;
using Pash.Implementation;
using Extensions.Dictionary;

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
            var funs = ExecutionContext.SessionState.Function.GetAllLocal();
            var vars = ExecutionContext.SessionState.PSVariable.GetAllLocal();
            var aliases = ExecutionContext.SessionState.Alias.GetAllLocal();
            var cmdlets = ExecutionContext.SessionState.Cmdlet.GetAllLocal();
            // resolve all function, variable, cmdlet, and alias patterns and add the matched members to the exports
            _affectedModule.ExportedFunctions.ReplaceContents(WildcardPattern.FilterDictionary(Function, funs));
            _affectedModule.ExportedVariables.ReplaceContents(WildcardPattern.FilterDictionary(Variable, vars));
            _affectedModule.ExportedAliases.ReplaceContents(WildcardPattern.FilterDictionary(Alias, aliases));
            _affectedModule.ExportedCmdlets.ReplaceContents(WildcardPattern.FilterDictionary(Cmdlet, cmdlets));
        }

    }
}

