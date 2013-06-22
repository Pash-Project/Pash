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
using Pash.ParserIntrinsics;

namespace Pash.Implementation
{
    internal class CommandManager
    {
        public struct SnapinProviderPair
        {
            public PSSnapInInfo snapinInfo;
            public Type providerType;
            public CmdletProviderAttribute providerAttr;
        }

        private Dictionary<string, PSSnapInInfo> _snapins;
        private Dictionary<string, List<CmdletInfo>> _cmdLets;
        private Dictionary<string, ScriptInfo> _scripts;
        private Dictionary<string, AliasInfo> _aliases;
        internal Collection<SnapinProviderPair> _providers;
        private ExecutionContext _context;

        public CommandManager()
            : this(null)
        {
        }

        public CommandManager(ExecutionContext context)
        {
            _context = context;

            _snapins = new Dictionary<string, PSSnapInInfo>(StringComparer.CurrentCultureIgnoreCase);
            _cmdLets = new Dictionary<string, List<CmdletInfo>>(StringComparer.CurrentCultureIgnoreCase);
            _scripts = new Dictionary<string, ScriptInfo>(StringComparer.CurrentCultureIgnoreCase);
            _aliases = new Dictionary<string, AliasInfo>(StringComparer.CurrentCultureIgnoreCase);
            _providers = new Collection<SnapinProviderPair>();

            // if no execution scope is provided load all the initial settings from the config file
            if (context == null)
            {
                // TODO: move this to config.ps1
                foreach (var snapinTypeName in new[] { 
                    "System.Management.Automation.PSCorePSSnapIn, System.Management.Automation",
                    "Microsoft.PowerShell.PSUtilityPSSnapIn, Microsoft.PowerShell.Commands.Utility",
                    "Microsoft.Commands.Management.PSManagementPSSnapIn, Microsoft.PowerShell.Commands.Management",
                })
                {
                    var tmpProviders = new Collection<SnapinProviderPair>();

                    // Load all PSSnapin's
                    foreach (CmdletInfo cmdLetInfo in LoadCmdletsFromPSSnapin(snapinTypeName, out tmpProviders))
                    {
                        // Register PSSnapin
                        if (_snapins.ContainsKey(cmdLetInfo.PSSnapIn.Name))
                        {
                            _snapins.Add(cmdLetInfo.PSSnapIn.Name, cmdLetInfo.PSSnapIn);
                        }

                        // Copy all the found Cmdlets
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

                    foreach (SnapinProviderPair providerTypePair in tmpProviders)
                    {
                        _providers.Add(providerTypePair);
                    }
                }
            }
        }

        public void SetAlias(string name, string definition)
        {
            if (this._aliases.ContainsKey(name))
                this._aliases[name] = new AliasInfo(name, definition, this);
            else
                NewAlias(name, definition);
        }

        public void NewAlias(string name, string definition)
        {
            if (this._aliases.ContainsKey(name)) throw new Exception("duplicate alias");
            AliasInfo aliasInfo = new AliasInfo(name, definition, this);

            _aliases.Add(aliasInfo.Name, aliasInfo);
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

            if (commandInfo == null && _aliases.ContainsKey(cmdName))
            {
                commandInfo = _aliases[cmdName].ReferencedCommand;
            }

            if (commandInfo == null && _cmdLets.ContainsKey(cmdName))
            {
                commandInfo = _cmdLets[cmdName].First();
            }

            if (commandInfo == null && _scripts.ContainsKey(cmdName))
            {
                commandInfo = _scripts[cmdName];
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
            // TODO: find the CommandInfo from CommandManager

            List<CmdletInfo> cmdletsList;
            if (_cmdLets.TryGetValue(command, out cmdletsList))
            {
                if (cmdletsList.Count > 0)
                    return cmdletsList[0];
            }

            ScriptInfo scriptInfo;
            if (_scripts.TryGetValue(command, out scriptInfo))
                return scriptInfo;

            AliasInfo aliasInfo;
            if (_aliases.TryGetValue(command, out aliasInfo))
                return aliasInfo;

            // TODO: search functions (in a context?)

            return null;
        }

        internal IEnumerable<CommandInfo> FindCommands(string pattern)
        {
            return from List<CmdletInfo> cmdletInfoList in _cmdLets.Values
                   from CmdletInfo info in cmdletInfoList
                   select info;
        }

        // TODO: separate providers from cmdlets
        internal Collection<CmdletInfo> LoadCmdletsFromPSSnapin(string strType, out Collection<SnapinProviderPair> providers)
        {
            Type snapinType = Type.GetType(strType, true);
            Assembly assembly = snapinType.Assembly;

            PSSnapIn snapin = Activator.CreateInstance(snapinType) as PSSnapIn;

            PSSnapInInfo snapinInfo = new PSSnapInInfo(snapin.Name, false, string.Empty, assembly.GetName().Name, string.Empty, new Version(1, 0), null, null, null, snapin.Description, snapin.Vendor);

            var snapinProviderPairs = from Type type in assembly.GetTypes()
                                      where !type.IsSubclassOf(typeof(Cmdlet))
                                      where type.IsSubclassOf(typeof(CmdletProvider))
                                      from CmdletProviderAttribute providerAttr in type.GetCustomAttributes(typeof(CmdletProviderAttribute), true)
                                      select new SnapinProviderPair
                                        {
                                            snapinInfo = snapinInfo,
                                            providerType = type,
                                            providerAttr = providerAttr
                                        };

            providers = snapinProviderPairs.ToCollection();

            var cmdletInfos = from Type type in assembly.GetTypes()
                              where type.IsSubclassOf(typeof(Cmdlet))
                              from CmdletAttribute cmdletAttribute in type.GetCustomAttributes(typeof(CmdletAttribute), true)
                              select new CmdletInfo(cmdletAttribute.FullName, type, null, snapinInfo, _context);

            return cmdletInfos.ToCollection();
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
            
            // Here we use reflection for dynamic loading of Mono.Posix assembly and invocation of
            // the native access call. Reflection is used for cross-platform compatibility of
            // System.Management assembly. Once compiled, it'll use native API only when run on
            // Unix platform.
            var posix = Assembly.Load("Mono.Posix");
            var syscall = posix.GetType("Mono.Unix.Native.Syscall");
            var accessModes = posix.GetType("Mono.Unix.Native.AccessModes");
            
            var access = syscall.GetMethod("access");
            var x_ok = accessModes.GetField("X_OK");

            var result = access.Invoke(null, new[] { path, x_ok.GetRawConstantValue() });
            return result.Equals(0);
        }
    }
}
