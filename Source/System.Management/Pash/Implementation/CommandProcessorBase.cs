﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation.Runspaces;
using System.Management.Automation;

namespace Pash.Implementation
{
    internal abstract class CommandProcessorBase
    {
        private ExecutionContext _executionContext;
        // parameters collection (addParameter)

        internal CommandInfo CommandInfo { get; set; }
        internal PipelineCommandRuntime CommandRuntime { get; set; }
        internal ExecutionContext ExecutionContext {
            get
            {
                return _executionContext;
            }
            set
            {
                _executionContext = value;
                if (CommandRuntime != null)
                {
                    CommandRuntime.ExecutionContext = _executionContext;
                }
            }
        }
        internal CommandParameterCollection Parameters { get; private set; }

        internal CommandProcessorBase(CommandInfo cmdInfo)
        {
            CommandInfo = cmdInfo;
            Parameters = new CommandParameterCollection();
            CommandRuntime = new PipelineCommandRuntime(null);
        }

        internal void SetPipelineProcessor(PipelineProcessor pipelineProcessor)
        {
            CommandRuntime.PipelineProcessor = pipelineProcessor;
        }

        internal void AddParameter(object value)
        {
            Parameters.Add(null, value);
        }

        internal void AddParameter(string name, object value)
        {
            Parameters.Add(name, value);
        }

        public override string ToString()
        {
            return this.CommandInfo.ToString();
        }

        /// <summary>
        /// First phase of lifecycle. E.g. for binding parameters that take command-line input.
        /// </summary>
        public abstract void Prepare();

        /// <summary>
        /// Starting the processing. E.g. for the cmdlet's BeginProcessing method.
        /// At this point, the CommandRuntime is already set up and can be used.
        /// Note that all elements of the pipeline pass this phase before one element
        /// enters the next phase.
        /// </summary>
        public abstract void BeginProcessing();

        /// <summary>
        /// The phase in which the pipeline's input objects are processed.
        /// For a cmdlet this means that eah object is bound as parameter and
        /// then processed by the cmdlet's ProcessRecord method.
        /// </summary>
        public abstract void ProcessRecords();

        /// <summary>
        /// The last phase. This is for cmdlet's to clean up after themselves (EndProcessing method).
        /// Last but not least this is for cleanup of the CommandProcessor itself.
        /// </summary>
        public abstract void EndProcessing();
    }
}
