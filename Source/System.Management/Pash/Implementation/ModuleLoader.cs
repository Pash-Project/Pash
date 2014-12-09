using System;
using System.Management.Automation;
using System.Management;
using System.Linq;
using System.Reflection;

namespace Pash.Implementation
{
    internal class ModuleLoader
    {
        private readonly string[] _manifestExtensions = new string[] { ".psd1" };
        private readonly string[] _scriptExtensions = new string[] { ".psm1", ".ps1" };
        // TODO sburnicki: Make sure .exe is a valid module extension
        private readonly string[] _assemblyExtensions = new string[] { ".dll", ".exe" };

        private ExecutionContext _executionContext;

        internal ModuleLoader(ExecutionContext context)
        {
            _executionContext = context;
        }

        internal PSModuleInfo LoadModuleByName(string name, bool loadToGlobalScope)
        {
            // TODO: where do we handle FileNotFoundExceptions etc?
            var path = new Path(name);
            if (name.Contains(path.CorrectSlash) || path.HasExtension())
            {
                // check if it's already loaded
                var loadedModule = _executionContext.SessionState.LoadedModules.Get(path);
                if (loadedModule != null)
                {
                    return loadedModule;
                }
                // load it otherwise
                var moduleInfo = LoadModuleByPath(path);
                _executionContext.SessionState.LoadedModules.Add(moduleInfo, loadToGlobalScope ? "global" : "local");
                return moduleInfo;
            }
            // otherwise we'd need to look in our module paths for a module
            throw new NotImplementedException("Currently you can only a specific module file, not installed modules");
        }

        private PSModuleInfo LoadModuleByPath(Path path)
        {
            // ALWAYS derive from global scope: the Scope parameter only defines where stuff is imported to
            var sessionState = new SessionState(_executionContext.SessionStateGlobal.RootSessionState);
            sessionState.IsScriptScope = true;
            var moduleInfo = new PSModuleInfo(path, path.GetFileNameWithoutExtension(), sessionState);
            sessionState.SetModule(moduleInfo);

            var ext = path.GetExtension();
            if (_scriptExtensions.Contains(ext))
            {
                LoadScriptModule(moduleInfo, path); // actually load the script
                return moduleInfo;
            }
            else if (_assemblyExtensions.Contains(ext))
            {
                LoadAssemblyModule(moduleInfo, path);
                return moduleInfo;
            }
            // TODO: nicer error message if the extension is *really* unknown
            throw new MethodInvocationException("The extension '" + ext + "' is currently not supported");
        }

        private void LoadAssemblyModule(PSModuleInfo moduleInfo, Path path)
        {
            Assembly assembly = Assembly.LoadFrom(path);
            // load into the local session state of the module
            moduleInfo.SessionState.Cmdlet.LoadCmdletsFromAssembly(assembly, moduleInfo);
            moduleInfo.ValidateExportedMembers(true, false); // make sure cmdlets get exported
        }

        private void LoadScriptModule(PSModuleInfo moduleInfo, Path path)
        {
            // execute the script in it's own scope
            var scriptBlock = new ExternalScriptInfo(path.ToString(), ScopeUsages.CurrentScope).ScriptBlock;
            var originalContext = _executionContext;
            var scopedContext = _executionContext.Clone(moduleInfo.SessionState, ScopeUsages.CurrentScope);
            scopedContext.ErrorStream.Redirect(originalContext.ErrorStream); // make sure errors are passed
            try
            {
                // actually change the scope by changing the execution context
                // there should definitely be a nicer way for this #ExecutionContextChange
                _executionContext = scopedContext;
                _executionContext.CurrentRunspace.ExecutionContext = scopedContext;
                scriptBlock.Invoke(); // TODO: pass parameters if set
                moduleInfo.ValidateExportedMembers(false, true); // make sure members are exported
            }
            catch (ExitException e)
            {
                var exitCode = LanguagePrimitives.ConvertTo<int>(e.Argument);
                _executionContext.SetLastExitCodeVariable(exitCode);
                _executionContext.SetSuccessVariable(exitCode == 0);
            }
            finally
            {
                // restore original context / scope
                _executionContext.CurrentRunspace.ExecutionContext = originalContext;
                _executionContext = originalContext;
            }
        }
    }
}

