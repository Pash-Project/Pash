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

        private Dictionary<string, PSSnapInInfo> _snapins;
        private Dictionary<string, List<CmdletInfo>> _cmdLets;
        private Dictionary<string, ScriptInfo> _scripts;
        private ExecutionContext _context;

        public CommandManager(ExecutionContext context)
        {
            _context = context;

            _snapins = new Dictionary<string, PSSnapInInfo>(StringComparer.CurrentCultureIgnoreCase);
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

        internal void RegisterPSSnaping(PSSnapInInfo snapinInfo)
        {
            if (!_snapins.ContainsKey(snapinInfo.Name))
            {
                _snapins.Add(snapinInfo.Name, snapinInfo);
            }
        }

        private void LoadScripts()
        {
            foreach (ScriptConfigurationEntry entry in _context.RunspaceConfiguration.Scripts)
            {
                try
                {
                    _scripts.Add(entry.Name, new ScriptInfo(entry.Name, new ScriptBlock(PowerShellGrammar.ParseInteractiveInput(entry.Definition))));
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
            string cmdName = command.CommandText;

            CommandInfo commandInfo = null;

            if (command.CommandText == null)
            {
                commandInfo = new ScriptInfo("", command.ScriptBlockAst.GetScriptBlock());
            }

            if (command.IsScript)
            {
                // TODO: take care of the script commands
                throw new NotImplementedException(this.ToString());
            }

            if (commandInfo == null)
            {
                commandInfo = FindCommand(cmdName);
            }

            // TODO: if the command wasn't found should we treat is as a Script?
            if (commandInfo == null)
            {
                var parseTree = PowerShellGrammar.ParseInteractiveInput(command.CommandText);
                var statements = parseTree.EndBlock.Statements;
                if (statements.Count == 1 && statements.Single() is PipelineAst)
                {
                    var pipelineAst = statements.Single() as PipelineAst;
                    if (pipelineAst.PipelineElements.Count == 1 && pipelineAst.PipelineElements.Single() is CommandAst)
                    {
                        var commandAst = pipelineAst.PipelineElements.Single() as CommandAst;
                        if (commandAst.CommandElements.Count == 1 && commandAst.CommandElements.Single() is StringConstantExpressionAst)
                        {
                            var stringAst = commandAst.CommandElements.Single() as StringConstantExpressionAst;
                            var path = ResolveExecutablePath(stringAst.Value);
                            if (path == null)
                            {
                                throw new Exception(string.Format("Command '{0}' not found.", cmdName));
                            }

                            if (Path.GetExtension(path) == ".ps1")
                            {
                                // I think we should be using a ScriptFile parser, but this will do for now.
                                commandInfo = new ScriptInfo(path, new ScriptBlock(PowerShellGrammar.ParseInteractiveInput(File.ReadAllText(path))));
                            }
                            else
                            {
                                var commandName = Path.GetFileName(path);
                                var extension = Path.GetExtension(path);

                                commandInfo = new ApplicationInfo(commandName, path, extension);
                            }
                        }
                    }
                }

                if (commandInfo == null)
                    commandInfo = new ScriptInfo("", new ScriptBlock(parseTree));
            }

            if (commandInfo != null)
            {
                switch (commandInfo.CommandType)
                {
                    case CommandTypes.Application:
                        return new ApplicationProcessor((ApplicationInfo)commandInfo);

                    case CommandTypes.Cmdlet:
                        return new CommandProcessor(commandInfo as CmdletInfo);

                    case CommandTypes.Function:
                        // TODO: teat the function as a script
                        break;

                    case CommandTypes.Script:
                        return new ScriptProcessor(commandInfo as ScriptInfo);
                }
            }

            throw new Exception(string.Format("Command '{0}' not found.", cmdName));
        }

        internal CommandInfo FindCommand(string command)
        {
            if (_context.SessionState.Alias.Exists(command))
            {
                return _context.SessionState.Alias.Get(command).ReferencedCommand;
            }

            if (_cmdLets.ContainsKey(command))
            {
                return _cmdLets[command].First();
            }

            if (_scripts.ContainsKey(command))
            {
                return _scripts[command];
            }

            // TODO: search functions (in a context?)

            return null;
        }

        internal IEnumerable<CommandInfo> FindCommands(string pattern)
        {
            return from List<CmdletInfo> cmdletInfoList in _cmdLets.Values
                   from CmdletInfo info in cmdletInfoList
                   select info;
        }

        internal IEnumerable<CmdletInfo> LoadCmdletsFromAssembly(Assembly assembly, PSSnapInInfo snapinInfo = null)
        {
            return from Type type in assembly.GetTypes()
                   where type.IsSubclassOf(typeof(Cmdlet))
                   from CmdletAttribute cmdletAttribute in type.GetCustomAttributes(typeof(CmdletAttribute), true)
                   select new CmdletInfo(cmdletAttribute.FullName, type, null, snapinInfo, _context);
        }

        internal IEnumerable<CmdletInfo> LoadCmdletsFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            return from Assembly assembly in assemblies
                   from CmdletInfo cmdletInfo in LoadCmdletsFromAssembly(assembly)
                   select cmdletInfo;
        }

        internal void ImportModules(IEnumerable<ModuleSpecification> modules)
        {
            var assemblies = from ModuleSpecification module in modules
                             select Assembly.LoadFile(module.Name);

            foreach (CmdletInfo cmdletInfo in LoadCmdletsFromAssemblies(assemblies))
            {
                RegisterCmdlet(cmdletInfo);
            }
        }

        // HACK: all functions are currently stored as scripts. But I'm confused, so I don't know how to fix it.
        internal void SetFunction(/*FunctionInfo */ScriptInfo functionInfo)
        {
            this._scripts[functionInfo.Name] = functionInfo;
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
