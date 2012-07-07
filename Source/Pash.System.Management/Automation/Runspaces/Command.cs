using System;
using Pash.Implementation;

namespace System.Management.Automation.Runspaces
{
    public sealed class Command
    {
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
        {
            CommandText = command;
            IsScript = isScript;
            UseLocalScope = useLocalScope;
            Parameters = new CommandParameterCollection();
            MergeMyResult = PipelineResultTypes.None;
            MergeToResult = PipelineResultTypes.None;
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
