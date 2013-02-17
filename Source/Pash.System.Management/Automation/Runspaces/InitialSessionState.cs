// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;
using Microsoft.PowerShell.Commands;

namespace System.Management.Automation.Runspaces
{
    public class InitialSessionState
    {
        PSLanguageMode langmode;
        InitialSessionStateEntryCollection<SessionStateCommandEntry> sessionstatentry;

        protected InitialSessionState()
        {
            sessionstatentry = new InitialSessionStateEntryCollection<SessionStateCommandEntry>();
        }

        #region Properties

        public ApartmentState ApartmentState
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateAssemblyEntry> Assemblies
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual AuthorizationManager AuthorizationManager
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateCommandEntry> Commands
        {
            get
            {
                return sessionstatentry;
            }
        }

        public bool DisableFormatUpdates
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateFormatEntry> Formats
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PSLanguageMode LanguageMode
        {
            get
            {
                return langmode;
            }
            set
            {
                langmode = value;
            }
        }

        public ReadOnlyCollection<ModuleSpecification> Modules
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateProviderEntry> Providers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PSThreadOptions ThreadOptions
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public bool ThrowOnRunspaceOpenError
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateTypeEntry> Types
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool UseFullLanguageModeInDebugger
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateVariableEntry> Variables
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public InitialSessionState Clone()
        {
            throw new NotImplementedException();
        }

        public static InitialSessionState Create()
        {
            return new InitialSessionState();
        }

        public static InitialSessionState Create(string snapInName)
        {
            return new InitialSessionState();
        }

        public static InitialSessionState Create(string[] snapInNameCollection, out PSConsoleLoadException warning)
        {
            warning = null;
            return new InitialSessionState();
        }

        public static InitialSessionState CreateDefault()
        {
            InitialSessionState initialSessionState = new InitialSessionState();

            return initialSessionState;
        }

        public static InitialSessionState CreateDefault2()
        {
            InitialSessionState initialSessionState = new InitialSessionState();

            return initialSessionState;
        }

        public static InitialSessionState CreateFrom(string snapInPath, out PSConsoleLoadException warnings)
        {
            warnings = null;
            return new InitialSessionState();
        }
        public static InitialSessionState CreateFrom(string[] snapInPathCollection, out PSConsoleLoadException warnings)
        {
            warnings = null;
            return new InitialSessionState();
        }

        public static InitialSessionState CreateRestricted(SessionCapabilities sessionCapabilities)
        {
            InitialSessionState initialSessionState = new InitialSessionState();

            return initialSessionState;
        }

        public void ImportPSModule(string[] name)
        {
            throw new NotImplementedException();
        }

        public void ImportPSModulesFromPath(string path)
        {
            throw new NotImplementedException();
        }

        public PSSnapInInfo ImportPSSnapIn(string name, out PSSnapInException warning)
        {
            throw new NotImplementedException();
        }


    }
}
