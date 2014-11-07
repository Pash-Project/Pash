// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using Pash.Implementation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Extensions.Reflection;

namespace System.Management.Automation
{
    internal class CommandProcessor : CommandProcessorBase
    {
        private CmdletParameterBinder _argumentBinder;
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
            _argumentBinder = new CmdletParameterBinder(_cmdletInfo, Command);
            _argumentBinder.BindCommandLineParameters(Parameters);
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
                // TODO: determine the correct second arg: true if this commandProcessor is the first command in pipeline
                _argumentBinder.BindPipelineParameters(curInput, true);
                Command.DoProcessRecord();
            }
            _argumentBinder.RestoreCommandLineParameterValues();
        }

        /// <summary>
        /// In the cleanup phase, the command's "EndProcessing" method will be called to do own cleanup
        /// </summary>
        public override void EndProcessing()
        {
            Command.DoEndProcessing();
            ExecutionContext.SetVariable("global:?", true); // only false if we got an exception
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
                var hasValidName = !String.IsNullOrEmpty(current.Name);
                // if we have a switch parameter, set default value or pre-convert to bool if numeric
                if (hasValidName && IsSwitchParameter(current.Name))
                {
                    // if null (=unset) it's true; even if it's explicitly set to null
                    object value = current.Value;
                    if (current.Value == null)
                    {
                        Parameters.Add(current.Name, true);
                        continue;
                    }
                    // strange thing in PS: While LanguagePrimitives aren't able to convert from numerics to
                    // SwitchParameter, it does work with parameters. So we check it here
                    else if (PSObject.Unwrap(current.Value).GetType().IsNumeric())
                    {
                        value = LanguagePrimitives.ConvertTo<bool>(current.Value);
                    }
                    Parameters.Add(current.Name, value);
                }
                // if the current parameter has no argument (and not explicilty set to null), try to merge the next
                else if (peek != null &&
                         hasValidName &&
                         current.Value == null &&

                         String.IsNullOrEmpty(peek.Name))
                {
                    Parameters.Add(current.Name, peek.Value);
                    i++; // skip next element as it was merged
                }
                // otherwise we have a usual parameter/argument set
                else
                {
                    Parameters.Add(current);
                }
            }
        }

        private bool IsSwitchParameter(string name)
        {
            try
            {
                var parameterInfo = _cmdletInfo.LookupParameter(name);
                return parameterInfo.ParameterType == typeof(SwitchParameter);
            }
            catch (ParameterBindingException)
            {
                // seems to be ambiguous. well, this is the wrong place to throw the error, it will be thrown later on
            }
            return false;
        }
    }
}
