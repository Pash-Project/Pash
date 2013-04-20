﻿// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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

        List<CommandProcessorBase> commandsToExecute;

        public PipelineProcessor()
        {
            commandsToExecute = new List<CommandProcessorBase>();
        }

        public void Add(CommandProcessorBase commandProcessor)
        {
            commandsToExecute.Add(commandProcessor);
        }

        public Array Execute(ExecutionContext context)
        {
            PSObject psObjectCurrent = context.inputStreamReader.Read();
            Collection<PSObject> dataCollection;

            do
            {
                dataCollection = new Collection<PSObject>() { psObjectCurrent };
                foreach (CommandProcessorBase commandProcessor in commandsToExecute)
                {
                    PipelineCommandRuntime commandRuntime = new PipelineCommandRuntime(this);

                    foreach (PSObject psObject in dataCollection)
                    {
                        // TODO: protect the execution context
                        commandProcessor.ExecutionContext = context;

                        // TODO: replace the Command default runtime to execute callbacks on the pipeline when the object is written to the pipeline and then execute the next cmdlet

                        // TODO: provide a proper command initialization (for parameters and pipeline objects)
                        commandProcessor.CommandRuntime = commandRuntime;

                        commandProcessor.BindArguments(psObject);

                        // TODO: for each entry in pipe
                        // Execute the cmdlet at least once (even if there were nothing in the pipe
                        commandProcessor.ProcessRecord();
                    }
                    commandProcessor.Complete();

                    // TODO: process Error stream

                    dataCollection = new PSObjectPipelineReader(commandRuntime.outputResults).ReadToEnd();
                }
            } while ((psObjectCurrent = context.inputStreamReader.Read()) != null);

            // Write the final result to the output pipeline
            context.outputStreamWriter.Write(dataCollection, true);

            return dataCollection.Cast<object>().ToArray();
        }
    }
}
