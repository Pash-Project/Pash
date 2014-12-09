using System;
using System.Management.Automation;
using System.Management;
using System.Linq;
using System.Reflection;
using System.Management.Pash.Implementation;
using System.Collections.ObjectModel;
using System.Collections;
using System.Security.Cryptography;
using Extensions.Dictionary;

namespace Pash.Implementation
{
    internal class ModuleLoader
    {
        private readonly string[] _manifestExtensions = new string[] { ".psd1" };
        private readonly string[] _scriptExtensions = new string[] { ".psm1", ".ps1" };
        // TODO sburnicki: Make sure .exe is a valid module extension
        private readonly string[] _binaryExtensions = new string[] { ".dll", ".exe" };

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
            sessionState.PSVariable.Set("PSScriptRoot", path.GetDirectory());
            var moduleInfo = new PSModuleInfo(path, path.GetFileNameWithoutExtension(), sessionState);
            sessionState.SetModule(moduleInfo);

            LoadFileIntoModule(moduleInfo, path);
            return moduleInfo;
        }

        private void LoadFileIntoModule(PSModuleInfo moduleInfo, Path path)
        {
            // prevents accidental loops while loading a module
            moduleInfo.NestingDepth++;
            if (moduleInfo.NestingDepth > 10)
            {
                var msg = "The module is too deeply nested. A module can be only nested 10 times. Make sure to check" +
                    " the loading order of your module";
                throw new PSInvalidOperationException(msg, "Modules_ModuleTooDeeplyNested",
                    ErrorCategory.InvalidOperation);
            }

            var stringComparer = StringComparer.InvariantCultureIgnoreCase;
            var ext = path.GetExtension();
            if (_scriptExtensions.Contains(ext, stringComparer))
            {
                moduleInfo.ModuleType = ModuleType.Script;
                LoadScriptModule(moduleInfo, path); // actually load the script
            }
            else if (_binaryExtensions.Contains(ext, stringComparer))
            {
                moduleInfo.ModuleType = ModuleType.Binary;
                LoadBinaryModule(moduleInfo, path);
            }
            else if (_manifestExtensions.Contains(ext, stringComparer))
            {
                moduleInfo.ModuleType = ModuleType.Manifest;
                LoadManifestModule(moduleInfo, path);
            }
            else
            {
                // TODO: nicer error message if the extension is *really* unknown
                throw new MethodInvocationException("The extension '" + ext + "' is currently not supported");
            }
            moduleInfo.ValidateExportedMembers();
        }

        private void LoadBinaryModule(PSModuleInfo moduleInfo, Path path)
        {
            Assembly assembly = Assembly.LoadFrom(path);
            // load into the local session state of the module
            moduleInfo.SessionState.Cmdlet.LoadCmdletsFromAssembly(assembly, moduleInfo);
            moduleInfo.ValidateExportedMembers(); // make sure cmdlets get exported
        }

        private void LoadScriptModule(PSModuleInfo moduleInfo, Path path)
        {
            // execute the script in the module scope
            // TODO: simply discard the module output?
            ExecuteScriptInSessionState(path.ToString(), moduleInfo.SessionState);
        }

        private void LoadManifestModule(PSModuleInfo moduleInfo, Path path)
        {
            // Load the manifest (hashtable). We use a isolated SessionState to not affect the existing one
            var isolatedSessionState = new SessionState(_executionContext.SessionStateGlobal);
            var res = ExecuteScriptInSessionState(path.ToString(), isolatedSessionState);
            if (res.Count != 1 || !(res[0].BaseObject is Hashtable))
            {
                var msg = "The module manifest is invalid as it doesn't simply define a hashtable. ";
                throw new PSInvalidOperationException(msg, "Modules_InvalidManifest", ErrorCategory.InvalidOperation);
            }
            var manifest = (Hashtable)res[0].BaseObject;

            // Load the metadata into the PSModuleInfo (simply overwrite if existing due to nesting)
            try
            {
                moduleInfo.SetMetadata(manifest);
            } 
            catch (PSInvalidCastException e)
            {                
                var msg = "The module manifest contains invalid data." + Environment.NewLine + e.Message;
                throw new PSInvalidOperationException(msg, "Modules_InvalidManifestData", ErrorCategory.InvalidData, e);
            }

            // TODO: check and load required assemblies and modules

            // Load RootModule / ModuleToProcess
            LoadManifestRootModule(moduleInfo, manifest);

            // restrict the module exports
            RestrictModuleExportsByManifest(moduleInfo, manifest);
        }

        private void LoadManifestRootModule(PSModuleInfo moduleInfo, Hashtable manifest)
        {

            var rootModule = manifest["RootModule"];
            var moduleToProcess = manifest["ModuleToProcess"];
            if (rootModule != null && moduleToProcess != null)
            {
                var msg = "The module manifest cannot contain both a definition of ModuleToProcess and RootModule.";
                // lol, this is the actual error id from PS:
                var id = "Modules_ModuleManifestCannotContainBothModuleToProcessAndRootModule";
                throw new PSInvalidOperationException(msg, id, ErrorCategory.InvalidOperation);
            }
            if (rootModule == null && moduleToProcess == null)
            {
                return;
            }
            rootModule = rootModule == null ? moduleToProcess : rootModule; // decide which one to use

            string rootModuleName;
            if (!LanguagePrimitives.TryConvertTo<string>(rootModule, out rootModuleName))
            {
                var msg = "The RootModule / ModuleToProcess value must be a string.";
                throw new PSInvalidOperationException(msg, "Modules_InvalidManifestRootModule",
                    ErrorCategory.InvalidData);
            }
            try
            {
                LoadFileIntoModule(moduleInfo, rootModuleName);
            }
            catch (PSInvalidOperationException e)
            {
                var errId = "Modules_FailedToLoadNestingModule";
                if (e.ErrorRecord.ErrorId.Equals(errId))
                {
                    throw; // this avoids that nested modules result in nested exceptions
                }
                var msg = "Failed to load the nesting module '" + rootModuleName + "'." +
                    Environment.NewLine + e.Message;
                throw new PSInvalidOperationException(msg, errId, ErrorCategory.InvalidOperation, e);
            }
        }

        void RestrictModuleExportsByManifest(PSModuleInfo moduleInfo, Hashtable manifest)
        {
            string[] funs, vars, aliases, cmdlets;
            try
            {
                funs = LanguagePrimitives.ConvertTo<string[]>(manifest["FunctionsToExport"]);
                vars = LanguagePrimitives.ConvertTo<string[]>(manifest["VariablesToExport"]);
                cmdlets = LanguagePrimitives.ConvertTo<string[]>(manifest["CmdletsToExport"]);
                aliases = LanguagePrimitives.ConvertTo<string[]>(manifest["AliasesToExport"]);
            }
            catch (PSInvalidCastException e)
            {
                var msg = "The module manifest contains invalid definitions for FunctionsToExport, VariablesToExport, "
                          + "CmdletsToExport, or AliasesToExport. Make sure they are either strings or string arrays";
                throw new PSInvalidOperationException(msg, "Modules_ManifestMembersToExportAreInvalid", 
                    ErrorCategory.InvalidData, e);
            }
            moduleInfo.ExportedFunctions.ReplaceContents(
                WildcardPattern.FilterDictionary(funs, moduleInfo.ExportedFunctions));
            moduleInfo.ExportedVariables.ReplaceContents(
                WildcardPattern.FilterDictionary(vars, moduleInfo.ExportedVariables));
            moduleInfo.ExportedAliases.ReplaceContents(
                WildcardPattern.FilterDictionary(aliases, moduleInfo.ExportedAliases));
            moduleInfo.ExportedCmdlets.ReplaceContents(
                WildcardPattern.FilterDictionary(cmdlets, moduleInfo.ExportedCmdlets));
        }

        private Collection<PSObject> ExecuteScriptInSessionState(string path, SessionState sessionState)
        {
            var scriptBlock = new ExternalScriptInfo(path, ScopeUsages.CurrentScope).ScriptBlock;
            var originalContext = _executionContext;
            var scopedContext = _executionContext.Clone(sessionState, ScopeUsages.CurrentScope);
            try
            {
                // actually change the scope by changing the execution context
                // there should definitely be a nicer way for this #ExecutionContextChange
                _executionContext = scopedContext;
                _executionContext.CurrentRunspace.ExecutionContext = scopedContext;
                return scriptBlock.Invoke(); // TODO: pass parameters if set
            }
            catch (ExitException e)
            {
                var exitCode = LanguagePrimitives.ConvertTo<int>(e.Argument);
                _executionContext.SetLastExitCodeVariable(exitCode);
                _executionContext.SetSuccessVariable(exitCode == 0);
                return new Collection<PSObject>() { PSObject.AsPSObject(e.Argument) };
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

