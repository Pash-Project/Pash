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

        internal PSModuleInfo LoadModuleByName(string name, bool loadToGlobalScope)
        {
            var path = new Path(name);
            if (name.Contains(path.CorrectSlash) || path.HasExtension())
            {
                // ALWAYS derive from global scope: the Scope parameter only defines where stuff is imported to
                var sessionState = new SessionState(SessionState.SessionStateGlobal.RootSessionState);
                sessionState.IsScriptScope = true;

                // check if it's already loaded
                var loadedModule = ExecutionContext.SessionState.LoadedModules.Get(path);
                if (loadedModule != null)
                {
                    return loadedModule;
                }

                // load it otherwise
                var moduleInfo = new PSModuleInfo(path, path.GetFileNameWithoutExtension(), sessionState);
                sessionState.SetModule(moduleInfo);
                LoadModuleByPath(moduleInfo, path);
                ExecutionContext.SessionState.LoadedModules.Add(moduleInfo, loadToGlobalScope ? "global" : "local");
                return moduleInfo;
            }
            // otherwise we'd need to look in our module paths for a module
            throw new NotImplementedException("Currently you can only a specific module file, not installed modules");
        }

        internal void LoadModuleByPath(PSModuleInfo moduleInfo, Path path)
        {
            var ext = path.GetExtension();
            if (_scriptExtensions.Contains(ext))
            {
                LoadScriptModule(moduleInfo, path); // actually load the script
                moduleInfo.ValidateExportedMembers(false, true); // make sure members are exported
                return;
            }
            // TODO: nicer error message if the extension is *really* unknown
            var exception = new MethodInvocationException("The extension '" + ext + "' is currently not supported");
            var error = new ErrorRecord(exception, "UnsupportedModuleExtension", ErrorCategory.SecurityError, path);
            ThrowTerminatingError(error);
        }

        internal void LoadScriptModule(PSModuleInfo moduleInfo, Path path)
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
                var exitCode = LanguagePrimitives.ConvertTo<int>(e.Argument);
                ExecutionContext.SetLastExitCodeVariable(exitCode);
                ExecutionContext.SetSuccessVariable(exitCode == 0);
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

