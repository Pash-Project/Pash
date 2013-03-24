// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation.Provider;
using System.Reflection;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using Pash.Implementation;
using Pash.ParserIntrinsics;
using Irony.Parsing;
using System.IO;
using System.Management.Automation.Language;
using Extensions.Enumerable;

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
                    "Microsoft.Commands.Management.PSManagementPSSnapIn, Microsoft.Commands.Management",
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

                            if (File.Exists(stringAst.Value) && Path.GetExtension(stringAst.Value) == ".ps1")
                            {
                                // I think we should be using a ScriptFile parser, but this will do for now.
                                commandInfo = new ScriptInfo(stringAst.Value, new ScriptBlock(PowerShellGrammar.ParseInteractiveInput(File.ReadAllText(stringAst.Value))));
                            }

                            else
                            {
                                throw new Exception(string.Format("Command '{0}' not found.", cmdName));
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
    }

}
