// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.PowerShell.Commands;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Reflection;
using Extensions.Enumerable;
using System.Text.RegularExpressions;
using System.Management.Automation.Runspaces;

namespace Pash.Implementation
{

    internal class SessionStateGlobal
    {
        //TODO: This and related stuff should be moved to InitialSessionState.CreateDefault
        // Key is assembly qualified pssnapin name, value is "required" (true) or optional (false)
        private Dictionary<string, bool> _defaultSnapins = new Dictionary<string, bool> {
            { "Microsoft.PowerShell.PSCorePSSnapIn, System.Management.Automation", true },
            { "Microsoft.PowerShell.PSUtilityPSSnapIn, Microsoft.PowerShell.Commands.Utility", false },
            { "Microsoft.Commands.Management.PSManagementPSSnapIn, Microsoft.PowerShell.Commands.Management", false },
            { "Microsoft.PowerShell.PSSecurityPSSnapin, Microsoft.PowerShell.Security", false }
        };
        private Dictionary<string, PSSnapInInfo> _snapins;
        private readonly ExecutionContext _globalExecutionContext;

        internal SessionState RootSessionState  { get { return _globalExecutionContext.SessionState; } }
        internal CmdletProviderManagementIntrinsics Provider { get; private set; }

        public PSDriveInfo CurrentDrive { get; internal set; }

        internal SessionStateGlobal(ExecutionContext executionContext)
        {
            _globalExecutionContext = executionContext;
            _snapins = new Dictionary<string, PSSnapInInfo>(StringComparer.CurrentCultureIgnoreCase);
            Provider = new CmdletProviderManagementIntrinsics(this);
        }


        #region PSSnapIn specific stuff
        internal void LoadDefaultPSSnapIns()
        {
            foreach (var snapinPair in _defaultSnapins)
            {
                var defaultSnapin = snapinPair.Key;
                var required = snapinPair.Value;
                Type snapinType = Type.GetType(defaultSnapin, false);
                if (snapinType == null)
                {
                    if (!required)
                    {
                        continue;
                    }
                    throw new TypeLoadException("Couldn't load type for required default snapin '" + defaultSnapin + "'");
                }
                PSSnapIn snapin = Activator.CreateInstance(snapinType) as PSSnapIn;
                Assembly snapinAssembly = snapinType.Assembly;
                LoadPSSnapIn(new PSSnapInInfo(snapin, snapinAssembly, true), snapinAssembly, _globalExecutionContext);
            }
        }

        internal Collection<PSSnapInInfo> GetPSSnapIns(WildcardPattern wildcard)
        {
            Collection<PSSnapInInfo> matches = new Collection<PSSnapInInfo>();
            foreach (var pair in _snapins)
            {
                if (wildcard.IsMatch(pair.Key))
                {
                    matches.Add(pair.Value);
                }
            }
            return matches;
        }

        internal PSSnapInInfo GetPSSnapIn(string name)
        {
            ValidatePSSnapInName(name);
            if (!_snapins.ContainsKey(name))
            {
                return null;
            }
            return _snapins[name];
        }

        internal PSSnapInInfo RemovePSSnapIn(string name, ExecutionContext context)
        {
            ValidatePSSnapInName(name);
            if (!_snapins.ContainsKey(name))
            {
                throw new PSArgumentException(String.Format("PSSnapIn '{0}' is not loaded!", name));
            }
            var snapinInfo = _snapins[name];
            if (snapinInfo.IsDefault)
            {
                throw new PSSnapInException(String.Format("PSSnapIn '{0}' is a built-in snapin and connot be removed!",
                                                          name));
            }
            //unload providers and associated drives
            RootSessionState.Provider.Remove(snapinInfo, context);
            
            //unload cmdlets
            RootSessionState.Cmdlet.RemoveAll(snapinInfo);

            _snapins.Remove(name);
            return snapinInfo;
        }

        /// <summary>
        /// Adds the PSSnapIn by name/path
        /// </summary>
        /// <exception cref="PSArgumentException">If loading the snapin fails (e.g. invalid name/path)</exception>
        /// <param name="name">Either the name of the registered snapin or a path to an assembly</param>
        /// <returns>The newly added PSSnapIn</returns>
        internal PSSnapInInfo AddPSSnapIn(string name, ExecutionContext context)
        {
            // a slash is not part of a valid snapin name. If the name contains a slash (e.g. "./foobar") its likely
            // that the user wants to load an assembly directly
            Assembly assembly = null;
            if (name.Contains(PathIntrinsics.CorrectSlash) || name.Contains(PathIntrinsics.WrongSlash))
            {
                assembly = LoadAssemblyFromFile(name);
            }
            else
            {
                assembly = LoadRegisteredPSSnapInAssembly(name);
            }
            //load snapins from assembly and make sure it's only one snapin class defined in thwere
            var snapins = from Type type in assembly.GetTypes()
                          where type.IsSubclassOf(typeof(PSSnapIn))
                          select type;
            if (snapins.Count() != 1)
            {
                string errorMsg = "The assembly '{0}' contains either no or more than one PSSnapIn class!";
                throw new PSSnapInException(String.Format(errorMsg, assembly.FullName));
            }
            PSSnapIn snapin = (PSSnapIn) Activator.CreateInstance(snapins.First());
            //okay, we got the new snapin. now load it
            PSSnapInInfo snapinInfo = new PSSnapInInfo(snapin, assembly, false);
            LoadPSSnapIn(snapinInfo, assembly, context);
            return snapinInfo;
        }

        private void LoadPSSnapIn(PSSnapInInfo snapinInfo, Assembly assembly, ExecutionContext context)
        {
            var snapinName = snapinInfo.Name;
            try
            {
                _snapins.Add(snapinName, snapinInfo);
            }
            catch (ArgumentException)
            {
                throw new PSSnapInException(String.Format("The snapin '{0}' is already loaded!", snapinName));
            }
            RootSessionState.Provider.Load(assembly, context, snapinInfo);
            RootSessionState.Cmdlet.LoadCmdletsFromAssembly(assembly, snapinInfo);
        }

        Assembly LoadAssemblyFromFile(string name)
        {
            try
            {
                return Assembly.LoadFrom(name);
            }
            catch (System.IO.FileNotFoundException e)
            {
                throw new PSArgumentException("The assembly was not found!", e);
            }
            catch (Exception e)
            {
                throw new PSSnapInException(String.Format("Loading the assembly '{0}' failed!", name), e);
            }
        }

        Assembly LoadRegisteredPSSnapInAssembly(string name)
        {
            ValidatePSSnapInName(name);
            throw new NotImplementedException();
            //TODO: support snapins registered in registry: read registry, get assembly path and load with
            //return LoadAssemblyFromFile(name);
        }

        private void ValidatePSSnapInName(string name)
        {
            if (!Regex.IsMatch(name, "[a-zA-Z0-9._]+"))
            {
                throw new PSArgumentException(String.Format("Invalid PSSnapIn name '{0}'", name));
            }
        }
        #endregion

        #region Current Location Management
        internal PathInfo CurrentLocation
        {
            get
            {
                if (CurrentDrive == null)
                {
                    throw new NullReferenceException("CurrentDrive is null.");
                }
                PSDriveInfo driveInfo = CurrentDrive;
                return new PathInfo(driveInfo, driveInfo.CurrentLocation, _globalExecutionContext.SessionState);
            }
        }

        internal void SetCurrentDrive()
        {
            ProviderInfo fileSystemProvider = RootSessionState.Provider.GetOne(FileSystemProvider.ProviderName);
            if (fileSystemProvider == null)
                throw new Exception("FileSystemProvider not found.");

            // Initialize the _currentDrive and set it's currentlocation 
            // to the same as the current environment directory
            foreach (var drive in fileSystemProvider.Drives)
            {
                Path currentDir = System.Environment.CurrentDirectory;
                if (string.Equals(drive.Name, currentDir.GetDrive(), StringComparison.OrdinalIgnoreCase))
                {
                    drive.CurrentLocation = currentDir;
                    CurrentDrive = drive;
                    return;
                }
            }

            throw new NotImplementedException("why are we here?");
        }
        #endregion

        #region PathIntrinsics
        //TODO: move this (and implement it) in the appropriate class, not here

        internal PathInfo CurrentProviderLocation(string providerName)
        {
            throw new NotImplementedException();
        }

        internal PathInfoStack LocationStack(string stackName)
        {
            throw new NotImplementedException();
        }

        internal PathInfo PopLocation(string stackName)
        {
            throw new NotImplementedException();
        }

        internal void PushCurrentLocation(string stackName)
        {
            throw new NotImplementedException();
        }

        internal PathInfoStack SetDefaultLocationStack(string stackName)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
