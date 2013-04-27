// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Pash.Implementation;
using System.Management.Automation.Language;

namespace System.Management.Automation.Runspaces
{
    public sealed class Command
    {
        internal readonly ScriptBlockAst ScriptBlockAst;

        readonly string _commandText;
        public string CommandText { get { return this._commandText; } }

        public bool IsScript { get; private set; }
        public bool UseLocalScope { get; private set; }

        public PipelineResultTypes MergeUnclaimedPreviousCommandResults { get; set; }
        public CommandParameterCollection Parameters { get; private set; }

        public Command(string command)
            : this(command, false, false)
        {
        }

        public Command(string command, bool isScript)
            : this(command, isScript, false)
        {
        }

        public Command(string command, bool isScript, bool useLocalScope)
            : this()
        {
            _commandText = command;
            IsScript = isScript;
            UseLocalScope = useLocalScope;
        }

        private Command()
        {
            Parameters = new CommandParameterCollection();
            MergeMyResult = PipelineResultTypes.None;
            MergeToResult = PipelineResultTypes.None;
        }

        internal Command(ScriptBlockAst scriptBlockAst)
            : this()
        {
            this.ScriptBlockAst = scriptBlockAst;
            IsScript = false;
        }

        public void MergeMyResults(PipelineResultTypes myResult, PipelineResultTypes toResult)
        {
            MergeMyResult = myResult;
            MergeToResult = toResult;
        }

        public override string ToString()
        {
            return CommandText;
        }

        // internals
        //internal Command Clone();
        //internal Command(Command command);
        internal CommandProcessorBase CreateCommandProcessor(ExecutionContext executionContext, CommandManager commandFactory, bool addToHistory)
        {
            CommandProcessorBase cmdProcBase = commandFactory.CreateCommandProcessor(this);
            cmdProcBase.ExecutionContext = executionContext;

            if ((Parameters != null) && (Parameters.Count > 0))
            {
                foreach (CommandParameter parameter in Parameters)
                {
                    if (string.IsNullOrEmpty(parameter.Name))
                    {
                        cmdProcBase.AddParameter(parameter.Value);
                    }
                    else
                    {
                        cmdProcBase.AddParameter(parameter.Name, parameter.Value);
                    }
                }
            }

            return cmdProcBase;
        }

        internal PipelineResultTypes MergeMyResult { get; private set; }
        internal PipelineResultTypes MergeToResult { get; private set; }
    }
}
