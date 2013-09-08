// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Host;
using System.Management.Automation;

namespace Pash.Implementation
{
    internal class LocalRunspace : Runspace
    {
        private InitialSessionState _initialSessionState;
        private RunspaceConfiguration _runspaceConfiguration;

        internal PSHost PSHost { get; set; }

        internal CommandManager CommandManager { get; private set; }

        // TODO: make sure to implement a Singleton DefaultRunspace pattern
        //internal static LocalRunspace DefaultRunspace { get; private set; }

        public LocalRunspace(PSHost host, RunspaceConfiguration configuration)
            : this(host, configuration, null)
        {

        }

        public LocalRunspace(PSHost host, InitialSessionState initialSessionState)
            : this(host, null, initialSessionState)
        {

        }

        internal LocalRunspace(PSHost host, RunspaceConfiguration configuration, InitialSessionState initialSessionState)
        {
            DefaultRunspace = this;
            PSHost = host;
            if (configuration == null)
                _runspaceConfiguration = RunspaceFactory.DefaultRunspaceConfiguration;
            else
                _runspaceConfiguration = configuration;
            ExecutionContext = new ExecutionContext(host, configuration);
            ExecutionContext.CurrentRunspace = this;
            _initialSessionState = initialSessionState;
        }

        public override InitialSessionState InitialSessionState
        {
            get
            {
                return this._initialSessionState;
            }
        }

        public override RunspaceConfiguration RunspaceConfiguration
        {
            get
            {
                return _runspaceConfiguration;
            }
        }

        public override RunspaceStateInfo RunspaceStateInfo
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        // unusued - NYI
        public override event EventHandler<RunspaceStateEventArgs> StateChanged = delegate { };

        public override Pipeline CreateNestedPipeline()
        {
            // TODO: make sure to fail if not Open
            return CreatePipeline();
        }

        internal override SessionStateProxy GetSessionStateProxy()
        {
            return new SessionStateProxy(this);
        }

        public object GetVariable(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Variable name can't be empty.");

            return ExecutionContext.GetVariable(name);
        }

        public void SetVariable(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("Variable name can't be empty.");

            ExecutionContext.SetVariable(name, value);
        }

        #region CreateXXX Pipeline
        public override Pipeline CreateNestedPipeline(string command, bool addToHistory)
        {
            // TODO: make sure to fail if not Open
            throw new NotImplementedException();
        }

        public override Pipeline CreatePipeline()
        {
            // TODO: make sure to fail if not Open
            return CreatePipeline("", false);
        }

        public override Pipeline CreatePipeline(string command)
        {
            // TODO: make sure to fail if not Open
            return CreatePipeline(command, false);
        }

        public override Pipeline CreatePipeline(string command, bool addToHistory)
        {
            // TODO: take care of the command history
            // TODO: make sure to fail if not Open
            LocalPipeline pipeline = new LocalPipeline(this, command);

            return pipeline;
        }
        #endregion

        #region OpenXXX Runspace
        public override void Open()
        {
            CommandManager = new CommandManager();
            InitializeSession();
            InitializeProviders();
        }

        public override void OpenAsync()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region CloseXXX Runspace
        public override void Close()
        {
        }

        public override void CloseAsync()
        {
            throw new NotImplementedException();
        }
        #endregion

        // internals
        internal void InitializeProviders()
        {
            ExecutionContext.SessionState.SessionStateGlobal.LoadProviders();
            ExecutionContext.SessionState.SessionStateGlobal.SetCurrentDrive();
        }

        private void InitializeSession()
        {
            if (_initialSessionState == null)
                return;

            AddInitialSessionVariables();
        }

        private void AddInitialSessionVariables()
        {
            foreach (SessionStateVariableEntry variableEntry in _initialSessionState.Variables)
            {
                SetVariable(variableEntry.Name, variableEntry.Value);
            }
        }
    }
}
