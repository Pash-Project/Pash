// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Pash.Implementation;

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
                _argumentBinder.RestoreCommandLineArguments();
                _argumentBinder.BindPipelineArguments(curInput);
                try
                {
                    _argumentBinder.CheckParameterSet();
                }
                catch (Exception e)
                {
                    var error = new ErrorRecord(e, "NotAllParametersProvided", ErrorCategory.InvalidOperation, null);
                    CommandRuntime.WriteError(error);
                    continue;
                }
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
    }
}
