// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Provider;
using System.Management.Automation.Runspaces;
using System.Reflection;
using Extensions.Enumerable;
using Pash.Implementation.Native;
using Pash.ParserIntrinsics;
using Microsoft.PowerShell.Commands;

namespace Pash.Implementation
{
    internal class CommandManager
    {
        //TODO: why do we support multiple commandlets with the same name? That really bugs me.
        private Dictionary<string, List<CmdletInfo>> _cmdLets;
        private Dictionary<string, ScriptInfo> _scripts;
        private LocalRunspace _runspace;

        public CommandManager(LocalRunspace runspace)
        {
            _runspace = runspace;

            _cmdLets = new Dictionary<string, List<CmdletInfo>>(StringComparer.CurrentCultureIgnoreCase);
            _scripts = new Dictionary<string, ScriptInfo>(StringComparer.CurrentCultureIgnoreCase);
        }

        internal void RegisterCmdlet(CmdletInfo cmdLetInfo)
        {
            List<CmdletInfo> cmdletList = null;
            if (_cmdLets.ContainsKey(cmdLetInfo.Name))
            {
                cmdletList = _cmdLets[cmdLetInfo.Name];
            }
            else
            {
                cmdletList = new List<CmdletInfo>();
                _cmdLets.Add(cmdLetInfo.Name, cmdletList);
            }
            cmdletList.Add(cmdLetInfo);
        }

        /// <summary>
        /// Removes all cmdlets of a specific PSSnapIn
        /// </summary>
        /// <param name="snapinInfo">Information about the PSSnapIn whose cmdlets should be removed</param>
        internal void RemoveCmdlets(PSSnapInInfo snapinInfo)
        {
            foreach (var pair in _cmdLets)
            {
                Collection <CmdletInfo> removables = new Collection<CmdletInfo>();
                foreach (var cmdlet in pair.Value)
                {
                    if (cmdlet.PSSnapIn == null) // not loaded with a snapin. e.g. with a module
                        continue;
                    if (cmdlet.PSSnapIn.Equals(snapinInfo))
                    {
                        removables.Add(cmdlet);
                    }
                }
                foreach (var rmCmdlet in removables)
                {
                    pair.Value.Remove(rmCmdlet);
                }
            }
        }

        private void LoadScripts()
        {
            foreach (ScriptConfigurationEntry entry in _runspace.ExecutionContext.RunspaceConfiguration.Scripts)
            {
                try
                {
                    var scriptBlock = new ScriptBlock(Parser.ParseInput(entry.Definition));
                    _scripts.Add(entry.Name, new ScriptInfo(entry.Name, scriptBlock,
                                                            ScopeUsages.NewScriptScope));
                    continue;
                }
                catch
                {
                    throw new Exception("DuplicateScriptName: " + entry.Name);
                }
            }
        }

        public CommandProcessorBase CreateCommandProcessor(Command command)
        {
            string cmdText = command.CommandText;

            CommandInfo commandInfo = null;
            bool useLocalScope = command.UseLocalScope;

            if (command.IsScript) //CommandText contains a script block. Parse it
            {
                command.ScriptBlockAst = Parser.ParseInput(cmdText);
            }

            //either we parsed something like "& { foo; bar }", or the CommandText was a script and was just parsed
            if (command.ScriptBlockAst != null)
            {
                commandInfo = new ScriptInfo("", command.ScriptBlockAst.GetScriptBlock(), 
                                             useLocalScope ? ScopeUsages.NewScope : ScopeUsages.CurrentScope);
            }
            else //otherwise it's a real command (cmdlet, script, function, application)
            {
                commandInfo = FindCommand(cmdText, useLocalScope);
            }
            // make sure we only create a valid command processor if it's a valid command
            commandInfo.Validate();

            switch (commandInfo.CommandType)
            {
                case CommandTypes.Application:
                    return new ApplicationProcessor((ApplicationInfo)commandInfo);

                case CommandTypes.Cmdlet:
                    return new CommandProcessor(commandInfo as CmdletInfo);

                case CommandTypes.Script:
                case CommandTypes.ExternalScript:
                case CommandTypes.Function:
                    return new ScriptBlockProcessor(commandInfo as IScriptBlockInfo, commandInfo);
                default:
                    throw new NotImplementedException("Invalid command type");
            }
        }

        internal CommandInfo FindCommand(string command, bool useLocalScope=true)
        {
            if (_runspace.ExecutionContext.SessionState.Alias.Exists(command))
            {
                var aliasInfo = _runspace.ExecutionContext.SessionState.Alias.Get(command);
                if (aliasInfo.ReferencedCommand == null)
                {
                    throw new CommandNotFoundException(string.Format("Command '{0}' not found.", aliasInfo.Definition));
                }
                return aliasInfo.ReferencedCommand;
            }

            CommandInfo function = _runspace.ExecutionContext.SessionState.Function.Get(command);
            if (function != null)
            {
                return function;
            }

            if (_cmdLets.ContainsKey(command) && _cmdLets[command].Any())
            {
                return _cmdLets[command].First();
            }

            if (_scripts.ContainsKey(command))
            {
                return _scripts[command];
            }

            var path = ResolveExecutablePath(command);
            if (path == null)
            {
                //This means it's neither a command name, nor there was a file that can be executed
                throw new CommandNotFoundException(string.Format("Command '{0}' not found.", command));
            }
            if (Path.GetExtension(path) == ".ps1")
            {
                return new ExternalScriptInfo(path, useLocalScope ? ScopeUsages.NewScriptScope : ScopeUsages.CurrentScope);
            }
            //otherwise it's an application
            return new ApplicationInfo(Path.GetFileName(path), path, Path.GetExtension(path));
        }

        internal IEnumerable<CommandInfo> FindCommands(string pattern)
        {
            var wildcard = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
            return from List<CmdletInfo> cmdletInfoList in _cmdLets.Values
                   from CmdletInfo info in cmdletInfoList
                   where wildcard.IsMatch(info.Name)
                   select info;
        }

        internal IEnumerable<CmdletInfo> LoadCmdletsFromAssembly(Assembly assembly, PSSnapInInfo snapinInfo = null)
        {
            var cmdlets = from Type type in assembly.GetTypes()
                   where type.IsSubclassOf(typeof(Cmdlet))
                   from CmdletAttribute cmdletAttribute in type.GetCustomAttributes(typeof(CmdletAttribute), true)
                   select new CmdletInfo(cmdletAttribute.FullName, type, null, snapinInfo, _runspace.ExecutionContext);
            foreach (CmdletInfo curCmdlet in cmdlets)
            {
                curCmdlet.AddCommonParameters();
                RegisterCmdlet(curCmdlet);
            }
            return cmdlets;
        }

        internal void LoadCmdletsFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (var curAssembly in assemblies)
            {
                LoadCmdletsFromAssembly(curAssembly);
            }
        }

        internal void ImportModules(IEnumerable<ModuleSpecification> modules)
        {
            //TODO: this module support is not good. we need a proper implementation that allows removing modules,
            //that remembers loaded modules and cares about public and private items in the modules
            var assemblies = from ModuleSpecification module in modules
                             select Assembly.LoadFile(module.Name);
            LoadCmdletsFromAssemblies(assemblies);
        }

        /// <summary>
        /// Resolves the path to the executable file.
        /// </summary>
        /// <param name="path">Relative path to the executable (with extension optionally omitted).</param>
        /// <returns>Absolute path to the executable file.</returns>
        private static string ResolveExecutablePath(string path)
        {
            // TODO: Forbid to run executables and scripts from the current directory without a leading slash.
            // For example, if current directory contains a 'file.exe' file, a command 'file.exe' should not
            // execute, but './file.exe' should.
            if (File.Exists(path))
            {
                return Path.GetFullPath(path);
            }

            // TODO: If not, then try to search the relative path with known extensions.

            // TODO: If path contains path separator (i.e. it pretends to be relative) and relative path not found, then
            // give up.

            return ResolveAbsolutePath(path);
        }

        /// <summary>
        /// Searches for the executable through system-wide directories.
        /// </summary>
        /// <param name="fileName">
        /// Absolute path to the executable (with extension optionally omitted). May be also from the PATH environment variable.
        /// </param>
        /// <returns>Absolute path to the executable file.</returns>
        private static string ResolveAbsolutePath(string fileName)
        {
            var systemPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var directories = SplitPaths(systemPath);

            var extensions = new List<string>{ ".ps1" }; // TODO: Clarify the priority of the .ps1 extension.
            
            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            if (isWindows)
            {
                var pathExt = Environment.GetEnvironmentVariable("PATHEXT") ?? string.Empty;
                extensions.AddRange(SplitPaths(pathExt));
            }

            if (!isWindows || extensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase))
            {
                // If file means to be executable without adding an extension, check it:
                var path = directories.Select(directory => Path.Combine(directory, fileName)).FirstOrDefault(File.Exists);
                if (path != null)
                {
                    return path;
                }
            }

            // Now search for file adding all extensions:
            var finalPath = extensions.Select(extension => fileName + extension)
                .SelectMany(
                    fileNameWithExtension => directories.Select(directory => Path.Combine(directory, fileNameWithExtension)))
                .FirstOrDefault(File.Exists);

            return finalPath;
        }

        /// <summary>
        /// Splits the combined path string using the <see cref="Path.PathSeparator"/> character.
        /// </summary>
        /// <param name="pathString">Path string.</param>
        /// <returns>Paths.</returns>
        private static string[] SplitPaths(string pathString)
        {
            return pathString.Split(new[]
            {
                Path.PathSeparator
            }, StringSplitOptions.RemoveEmptyEntries);
        }
        
        private static bool IsExecutable(string path)
        {
            return File.Exists(path)
                && (string.Equals(Path.GetExtension(path), ".ps1", StringComparison.OrdinalIgnoreCase)
                    || IsUnixExecutable(path));
        }
        
        private static bool IsUnixExecutable(string path)
        {
            var platform = Environment.OSVersion.Platform;
            if (platform != PlatformID.Unix && platform != PlatformID.MacOSX)
            {
                return false;
            }

            return Posix.access(path, Posix.X_OK) == 0;
        }
    }
}
