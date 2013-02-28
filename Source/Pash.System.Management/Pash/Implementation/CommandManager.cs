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

            if (!command.IsScript)
            {
                // check aliases
                if (_aliases.ContainsKey(cmdName))
                {
                    var aliasInfo = _aliases[cmdName];
                    commandInfo = aliasInfo.ReferencedCommand;
                }

                if (commandInfo == null)
                {
                    if (_cmdLets.ContainsKey(cmdName))
                    {
                        var cmdletInfoList = _cmdLets[cmdName];
                        if ((cmdletInfoList != null) && (cmdletInfoList.Count > 0))
                            commandInfo = cmdletInfoList[0];
                    }
                }

                if (commandInfo == null)
                {
                    if (_scripts.ContainsKey(cmdName))
                    {
                        commandInfo = _scripts[cmdName];
                    }
                }

                if (commandInfo == null)
                {
                    if (File.Exists(cmdName) && Path.GetExtension(cmdName) == ".ps1")
                    {
                        // I think we should be using a ScriptFile parser, but this will do for now.
                        commandInfo = new ScriptInfo(cmdName, new ScriptBlock(PowerShellGrammar.ParseInteractiveInput(File.ReadAllText(cmdName))));
                    }
                }
            }
            else
            {
                // TODO: take care of the script commands
                throw new NotImplementedException(this.ToString());
            }

            // TODO: if the command wasn't found should we treat is as a Script?
            if (commandInfo == null)
                commandInfo = new ScriptInfo("", new ScriptBlock(PowerShellGrammar.ParseInteractiveInput(command.CommandText)));

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

            throw new Exception(string.Format("Command {0} not found.", cmdName));
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

        internal List<CommandInfo> FindCommands(string pattern)
        {
            List<CommandInfo> commandsList = new List<CommandInfo>();

            foreach (List<CmdletInfo> cmdletInfoList in _cmdLets.Values)
            {
                foreach (CmdletInfo info in cmdletInfoList)
                {
                    commandsList.Add(info);
                }
            }

            return commandsList;
        }

        // TODO: separate providers from cmdlets
        internal Collection<CmdletInfo> LoadCmdletsFromPSSnapin(string strType, out Collection<SnapinProviderPair> providers)
        {
            Collection<CmdletInfo> collection = new Collection<CmdletInfo>();
            providers = new Collection<SnapinProviderPair>();

            try
            {
                Type snapinType = Type.GetType(strType, true);
                Assembly assembly = snapinType.Assembly;

                PSSnapIn snapin = Activator.CreateInstance(snapinType) as PSSnapIn;

                PSSnapInInfo snapinInfo = new PSSnapInInfo(snapin.Name, false, string.Empty, assembly.GetName().Name, string.Empty, new Version(1, 0), null, null, null, snapin.Description, snapin.Vendor);

                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(Cmdlet)))
                    {
                        foreach (CmdletAttribute cmdletAttribute in type.GetCustomAttributes(typeof(CmdletAttribute), true))
                        {
                            CmdletInfo cmdletInfo =
                                new CmdletInfo(cmdletAttribute.ToString(), type, null, snapinInfo, _context);
                            collection.Add(cmdletInfo);
                        }
                        continue;
                    }

                    if (type.IsSubclassOf(typeof(CmdletProvider)))
                    {
                        foreach (CmdletProviderAttribute providerAttr in type.GetCustomAttributes(typeof(CmdletProviderAttribute), true))
                        {
                            providers.Add(new SnapinProviderPair()
                            {
                                snapinInfo = snapinInfo,
                                providerType = type,
                                providerAttr = providerAttr
                            });
                        }
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return collection;
        }

        // HACK: all functions are currently stored as scripts. But I'm confused, so I don't know how to fix it.
        internal void SetFunction(/*FunctionInfo */ScriptInfo functionInfo)
        {
            this._scripts[functionInfo.Name] = functionInfo;
        }
    }

}
