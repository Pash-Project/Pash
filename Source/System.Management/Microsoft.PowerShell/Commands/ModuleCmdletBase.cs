using System;
using System.Linq;
using Pash.Implementation;
using System.Management.Pash.Implementation;

namespace System.Management.Automation
{
    public class ModuleCmdletBase : PSCmdlet
    {
        private readonly string[] _manifestExtensions = new string[] { ".psd1" };
        private readonly string[] _scriptExtensions = new string[] { ".psm1", ".ps1" };
        private readonly string[] _assemblyExtensions = new string[] { ".dll" };

        public PSModuleInfo LoadModuleByName(string name)
        {
            var path = new Path(name);
            if (name.Contains(path.CorrectSlash) || path.HasExtension())
            {
                // ALWAYS derive from global scope: the Scope parameter only defines where stuff is imported to
                var sessionState = new SessionState(SessionState.SessionStateGlobal.RootSessionState);
                sessionState.IsScriptScope = true;
                var moduleInfo = new PSModuleInfo(path.GetFileNameWithoutExtension(), sessionState);
                sessionState.SetModule(moduleInfo);

                LoadModuleByPath(moduleInfo, path);
                return moduleInfo;
            }
            throw new NotImplementedException("Currently you can only a specific module file, not installed modules");
        }

        public void LoadModuleByPath(PSModuleInfo moduleInfo, Path path)
        {
            var ext = path.GetExtension();
            if (_scriptExtensions.Contains(ext))
            {
                LoadScriptModule(moduleInfo, path); // actually load the script
                moduleInfo.ExportMembers(false, true); // make sure members are exported
                return;
            }
            // TODO: nicer error message if the extension is *really* unknown
            var exception = new MethodInvocationException("The extension '" + ext + "' is currently not supported");
            var error = new ErrorRecord(exception, "UnsupportedModuleExtension", ErrorCategory.SecurityError, path);
            ThrowTerminatingError(error);
        }

        public void LoadScriptModule(PSModuleInfo moduleInfo, Path path)
        {
            // execute the script in it's own scope
            var scriptBlock = new ExternalScriptInfo(path.ToString(), ScopeUsages.CurrentScope).ScriptBlock;
            var originalContext = ExecutionContext;
            var scopedContext = ExecutionContext.Clone(moduleInfo.SessionState, ScopeUsages.CurrentScope);
            scopedContext.ErrorStream.Redirect(originalContext.ErrorStream); // make sure errors are passed
            try
            {
                // actually change the scope by changing the execution context
                // there should definitely be a nicer way for this #ExecutionContextChange
                ExecutionContext = scopedContext;
                ExecutionContext.CurrentRunspace.ExecutionContext = scopedContext;
                scriptBlock.Invoke(); // TODO: pass parameters if set
            }
            catch (ExitException e)
            {
                ExecutionContext.SetLastExitCodeVariable(e.ExitCode);
                ExecutionContext.SetSuccessVariable(e.ExitCode == 0);
            }
            finally
            {
                // restore original context / scope
                ExecutionContext.CurrentRunspace.ExecutionContext = originalContext;
                ExecutionContext = originalContext;
            }
        }
    }
}

