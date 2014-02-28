// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Linq;
using System.Text;

namespace Pash.Implementation
{
    internal class PipelineProcessor
    {
        // TODO: implement pipeline stopping mechanism
        // Not really supposed to be `const`, that's just to clarify that this variable isn't currently used.
        private const bool _stopping = false;

        internal bool Stopping
        {
            get
            {
                return _stopping;
            }
        }

        private List<CommandProcessorBase> _commandsToExecute;

        public PipelineProcessor()
        {
            _commandsToExecute = new List<CommandProcessorBase>();
        }

        public void Add(CommandProcessorBase commandProcessor)
        {
            var commandCount = _commandsToExecute.Count;
            var commandInput = commandProcessor.CommandRuntime.InputStream;
            // redirect output of last command to new input
            if (commandCount > 0)
            {
                var lastCommand = _commandsToExecute[commandCount - 1];
                lastCommand.CommandRuntime.OutputStream.Redirect(commandInput);
            }
            // check if the command collets previous error results
            if (commandCount >  0 &&
                commandProcessor.CommandRuntime.MergeUnclaimedPreviousErrors)
            {
                foreach (var prevCommand in _commandsToExecute)
                {
                    if (prevCommand.CommandRuntime.ErrorStream.ClaimedBy == null)
                    {
                        prevCommand.CommandRuntime.ErrorStream.Redirect(commandInput);
                    }
                }
            }
            _commandsToExecute.Add(commandProcessor);
        }

        /// <summary>
        ///  Executes the pipeline with regard to the processing lifecycle.
        /// </summary>
        /// <param name="context">The current ExecutionContext</param>
        /// <remarks>
        /// Read more about the lifecycle at http://msdn.microsoft.com/en-us/library/ms714429(v=vs.85).aspx
        /// </remarks>
        public void Execute(ExecutionContext context)
        {

            if (!_commandsToExecute.Any())
            {
                return;
            }
            // redirect context input to first command's input
            _commandsToExecute[0].CommandRuntime.InputStream.Redirect(context.InputStream);
            // redirect output of last command to context
            _commandsToExecute[_commandsToExecute.Count - 1].CommandRuntime.OutputStream.Redirect(context.OutputStream);
            // set correct ExecutionContext
            // TODO: think about whether it's okay that everyone uses simply the same context
            foreach (var curCommand in _commandsToExecute)
            {
                curCommand.ExecutionContext = context;
            }

            /* PREPARE - bind all cli args
                    1. Bind the named parameters.
                    2. Bind the positional parameters.
                    3. Bind the common parameters.
                    4. Bind the parameters to support calls to the ShouldProcess method.
                    5. Bind the named dynamic parameters
                    6. Bind the positional dynamic parameters.
             */
            foreach (var curCommand in _commandsToExecute)
            {
                curCommand.Prepare();
            }

            // BEGIN - call all BeginProcessing methods
            foreach (var curCommand in _commandsToExecute)
            {
                curCommand.BeginProcessing();
            }

            /* PROCESS - process records from pipeline
                    1. Bind command-defined pipeline parameters.
                    2. Bind dynamic pipeline parameters.
                    3. Determine wether all necessary parameters are set
                    4. Call the real command's "ProcessRecord" method
             */
            foreach (var curCommand in _commandsToExecute)
            {
                curCommand.ProcessRecords();
            }

            // END - call all EndProcessing methods
            foreach (var curCommand in _commandsToExecute)
            {
                curCommand.EndProcessing();
            }
        }

    }
}
