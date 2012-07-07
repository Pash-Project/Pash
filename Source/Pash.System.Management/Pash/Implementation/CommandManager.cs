using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Provider;
using System.Reflection;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using Pash.Implementation;
using Pash.Configuration;

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

        public CommandManager() : this(null)
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
                ExecutionContextConfigurationSection configSection = System.Configuration.ConfigurationManager.GetSection("defaultExecutionContext") as ExecutionContextConfigurationSection;
                if (configSection != null)
                {
                    if (configSection.PSSnapins != null)
                    {
                        foreach (PSSnapinElement snapin in configSection.PSSnapins)
                        {
                            var tmpProviders = new Collection<SnapinProviderPair>();

                            // Load all PSSnapin's
                            foreach (CmdletInfo cmdLetInfo in LoadCmdletsFromPSSnapin(snapin.type, out tmpProviders))
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

                    // copy functions
                    if (configSection.Functions != null)
                    {
                        // TODO: resolve the difference between the function and a script?
                        foreach (FunctionElement function in configSection.Functions)
                        {
                            ScriptInfo scriptInfo = null;
                            switch (function.type)
                            {
                                case "inline":
                                    scriptInfo = new ScriptInfo(function.name, new ScriptBlock(context, function.value));
                                    break;

                                case "file":
                                    // TODO: read the function from a file
                                    break;
                            }

                            if (scriptInfo != null)
                                _scripts.Add(scriptInfo.Name, scriptInfo);
                        }
                    }
                    if (configSection.Aliases != null)
                    {
                        // TODO: cache aliases
                        foreach (AliasElement alias in configSection.Aliases)
                        {
                            AliasInfo aliasInfo = new AliasInfo(alias.name, alias.definition, this);

                            if (aliasInfo != null)
                                _aliases.Add(aliasInfo.Name, aliasInfo);
                        }
                    }
                    // fill variables into the execution scope
                    if (_context != null)
                    {
                        if (configSection.Variables != null)
                        {
                            // TODO: prepopulate variables list
                        }
                        // TODO: copy all the variables from the environment
                    }
                }
            }
        }

        private void LoadScripts()
        {
            foreach (ScriptConfigurationEntry entry in _context.RunspaceConfiguration.Scripts)
            {
                try
                {
                    _scripts.Add(entry.Name, new ScriptInfo(entry.Name, new ScriptBlock(_context, entry.Definition)));
                    continue;
                }
                catch
                {
                    throw new Exception("DuplicateScriptName: " + entry.Name);
                }
            }
        }

        public CommandInfo GetCommandInfo(string commandName)
        {
            // TODO: there can be more than one CmdLet with the same name
            CommandInfo commandInfo = null;

            if (commandInfo == null)
            {
                throw new CommandNotFoundException(commandName, null, "CommandNotFoundException", new object[0]);
            }

            return commandInfo;
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
            }
            else
            {
                // TODO: take care of the script commands
            }

            // TODO: if the command wasn't found should we treat is as a Script?
            if (commandInfo == null)
                commandInfo = new ScriptInfo("", new ScriptBlock(command.CommandText));

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

            if (_cmdLets.ContainsKey(command))
            {
                var cmdletsList = _cmdLets[command];
                if (cmdletsList.Count > 0)
                    return cmdletsList[0];
            }

            if (_scripts.ContainsKey(command))
                return _scripts[command];

            if (_aliases.ContainsKey(command))
                return _aliases[command];

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
                    if (type.IsSubclassOf(typeof (Cmdlet)))
                    {
                        foreach (CmdletAttribute cmdletAttribute in type.GetCustomAttributes(typeof (CmdletAttribute), true))
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
                            providers.Add(new SnapinProviderPair() {
                                snapinInfo = snapinInfo,
                                providerType = type,
                                providerAttr = providerAttr
                            });
                        }
                        continue;
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return collection;
        }
    }
 

}
