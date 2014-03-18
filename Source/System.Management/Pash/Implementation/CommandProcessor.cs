// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Pash.Implementation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace System.Management.Automation
{
    internal class CommandProcessor : CommandProcessorBase
    {
        private CmdletArgumentBinder _argumentBinder;
        internal Cmdlet Command { get; private set; }
        readonly CmdletInfo _cmdletInfo;
        bool _beganProcessing;

        public CommandProcessor(CmdletInfo cmdletInfo)
            : base(cmdletInfo)
        {
            _cmdletInfo = cmdletInfo;
            _beganProcessing = false;
        }

        /// <summary>
        /// First phase of cmdlet lifecycle: "Binding Parameters that Take Command-Line Input"
        /// </summary>
        public override void Prepare()
        {
            Cmdlet cmdlet = (Cmdlet)Activator.CreateInstance(_cmdletInfo.ImplementingType);
            cmdlet.CommandInfo = _cmdletInfo;
            cmdlet.ExecutionContext = base.ExecutionContext;
            cmdlet.CommandRuntime = CommandRuntime;
            Command = cmdlet;
            MergeParameters();
            _argumentBinder = new CmdletArgumentBinder(_cmdletInfo, Command);
            _argumentBinder.BindCommandLineArguments(Parameters);
        }

        /// <summary>
        /// Second phase. Basically calling the command's "BeginProcessing" method
        /// </summary>
        public override void BeginProcessing()
        {
            if (!_beganProcessing)
            {
                Command.DoBeginProcessing();
                _beganProcessing = true;
            }
        }

        /// <summary>
        /// In this phase, the "ProcessRecord" method of the command will be called for each
        /// object from the input pipeline, but at least once. Doing so, the input object
        /// will be bound as parameters, but only for the specific invocation.
        /// </summary>
        public override void ProcessRecords()
        {
            // check if we already called BeginProcessing for this command
            if (!_beganProcessing)
            {
                // this can happen if the previous element in the pipeline produces output in the BeginProcessing phase
                // than this command is asked to process the records but wasn't in the BeginProcessing phase, yet.
                BeginProcessing();
            }
            var inputObjects = CommandRuntime.InputStream.Read();
            foreach (var curInput in inputObjects)
            {
                // TODO: sburnicki - determine the correct second argument
                _argumentBinder.BindPipelineArguments(curInput, true);
                Command.DoProcessRecord();
            }
            _argumentBinder.RestoreCommandLineArguments();
        }

        /// <summary>
        /// In the cleanup phase, the command's "EndProcessing" method will be called to do own cleanup
        /// </summary>
        public override void EndProcessing()
        {
            Command.DoEndProcessing();
        }

        /// <summary>
        /// The parse currently adds each parameter without value and each value without parameter name, because
        /// it doesn't know at parse time whether the value belongs to the paramater or if the parameter is a switch
        /// parameter and the value is just a positional parameter. As we now know more abot the parameters, we can
        /// merge all parameters with the upcoming value if it's not a switch parameter
        /// </summary>
        private void MergeParameters()
        {
            var oldParameters = new Collection<CommandParameter>(new List<CommandParameter>(Parameters));
            Parameters.Clear();
            int numParams = oldParameters.Count;
            for (int i = 0; i < numParams; i++)
            {
                var current = oldParameters[i];
                var peek = (i < numParams - 1) ? oldParameters[i + 1] : null;
                if (peek != null &&
                    !String.IsNullOrEmpty(current.Name) &&
                    current.Value == null &&
                    !IsSwitchParameter(current.Name) &&
                    String.IsNullOrEmpty(peek.Name))
                {
                    Parameters.Add(current.Name, peek.Value);
                    i++; // skip next element as it was merged
                }
                else
                {
                    Parameters.Add(current);
                }
            }
        }

        private bool IsSwitchParameter(string name)
        {
            foreach (var curPair in _cmdletInfo.ParameterTypeLookupTable)
            {
                if (curPair.Key.StartsWith(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    // one match is enough, if there were multiple matches we'll get an error anyway
                    return curPair.Value == typeof(SwitchParameter);
                }
            }
            return false;
        }
    }
}
