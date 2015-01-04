// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Management.Pash.Implementation;

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

        internal void AddParameters(Collection<CommandParameter> parameters)
        {
            if (parameters == null)
            {
                return;
            }
            foreach (var param in parameters)
            {
                Parameters.Add(param);
            }
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
        /// First phase of lifecycle. E.g. for binding parameters that take command-line input. This happens
        /// before any command in the pipeline is executed and in this phase the command can not write to error/output
        /// </summary>
        public abstract void Prepare();

        /// <summary>
        /// Starting the processing. E.g. for the cmdlet's BeginProcessing method.
        /// At this point, the CommandRuntime is already set up and can be used.
        /// Usually, this phase is called before any element of the pipeline enters the ProcessRecord phase,
        /// unless some element produces output that needs to be directly consumed
        /// </summary>
        public abstract void BeginProcessing();

        /// <summary>
        /// The phase in which the pipeline's input objects are processed.
        /// For a cmdlet this means that eah object is bound as parameter and
        /// then processed by the cmdlet's ProcessRecord method.
        /// This method might be called several times, as soon as new input is available that needs to be processed
        /// </summary>
        public abstract void ProcessRecords();

        /// <summary>
        /// The last phase. This phase is executed directly after the ProcessRecords phase of this command.
        /// Only after this phase, the next command in the pipeline enters the ProcessRecords phase.
        /// This is for cmdlet's to clean up after themselves (EndProcessing method).
        /// Last but not least this is for cleanup of the CommandProcessor itself.
        /// </summary>
        public abstract void EndProcessing();

        internal RedirectionVisitor RedirectionVisitor { get; set; }

        internal void ProcessRedirects()
        {
            if (RedirectionVisitor != null)
            {
                RedirectionVisitor.Visit(CommandRuntime);
            }
        }
    }
}
