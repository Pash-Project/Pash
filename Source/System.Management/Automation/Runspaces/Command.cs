// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Pash.Implementation;
using System.Management.Automation.Language;
using System.Management.Pash.Implementation;

namespace System.Management.Automation.Runspaces
{
    public sealed class Command
    {
        internal ScriptBlockAst ScriptBlockAst { get; set; }

        public string CommandText { get; private set; }
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
            CommandText = command;
            IsScript = isScript;
            UseLocalScope = useLocalScope;
            // although the following lines are a kind of "hack", these might be the only location
            // to generally set this property for any occurence of "out-default"
            if (String.Equals(CommandText, "out-default", StringComparison.CurrentCultureIgnoreCase))
            {
                MergeUnclaimedPreviousCommandResults = PipelineResultTypes.Output | PipelineResultTypes.Error;
            }
            else
            {
                MergeUnclaimedPreviousCommandResults = PipelineResultTypes.None;
            }
        }

        private Command()
        {
            Parameters = new CommandParameterCollection();
            MergeMyResult = PipelineResultTypes.None;
            MergeToResult = PipelineResultTypes.None;
            ScriptBlockAst = null;
        }

        internal Command(ScriptBlockAst scriptBlockAst, bool useLocalScope = false)
            : this()
        {
            this.ScriptBlockAst = scriptBlockAst;
            this.UseLocalScope = useLocalScope;
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
            cmdProcBase.AddParameters(Parameters);
            SetMergeResultOptions(cmdProcBase);
            cmdProcBase.RedirectionVisitor = RedirectionVisitor;
            return cmdProcBase;
        }

        internal void SetMergeResultOptions(CommandProcessorBase procBase)
        {
            var rt = procBase.CommandRuntime;
            rt.MergeErrorToOutput = (MergeMyResult.Equals(PipelineResultTypes.Error) &&
                MergeToResult.Equals(PipelineResultTypes.Output));
            rt.MergeUnclaimedPreviousErrors = MergeUnclaimedPreviousCommandResults != PipelineResultTypes.None;
        }

        internal PipelineResultTypes MergeMyResult { get; private set; }
        internal PipelineResultTypes MergeToResult { get; private set; }

        internal RedirectionVisitor RedirectionVisitor { get; set; }
    }
}
