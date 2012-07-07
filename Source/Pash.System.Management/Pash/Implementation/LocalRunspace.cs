using System;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Host;
using System.Management.Automation;

namespace Pash.Implementation
{
    internal class LocalRunspace : Runspace
    {
        private RunspaceConfiguration _runspaceConfiguration;

        internal PSHost PSHost { get; set; }

        internal CommandManager CommandManager { get; private set; }

        // TODO: make sure to implement a Singleton DefaultRunspace pattern
        //internal static LocalRunspace DefaultRunspace { get; private set; }

        public LocalRunspace(PSHost host, RunspaceConfiguration configuration)
        {
            DefaultRunspace = this;
            PSHost = host;
            _runspaceConfiguration = configuration;
            ExecutionContext = new ExecutionContext(host, configuration);
            ExecutionContext.CurrentRunspace = this;
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

        public override event EventHandler<RunspaceStateEventArgs> StateChanged;

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
            throw new NotImplementedException();
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
    }
}
