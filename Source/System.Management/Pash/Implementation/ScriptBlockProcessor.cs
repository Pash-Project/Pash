// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Pash.Implementation;
using System.Management.Automation;
using Pash.ParserIntrinsics;
using Irony.Parsing;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Management.Pash.Implementation;

namespace Pash.Implementation
{
    internal class ScriptBlockProcessor : CommandProcessorBase
    {
        private readonly IScriptBlockInfo _scriptBlockInfo;
        private ExecutionContext _scopedContext;
        private ExecutionContext _originalContext;
        private bool _fromFile;
        private int? _exitCode;
        private ExecutionVisitor _scopedExecutionVisitor;
        ScriptBlockParameterBinder _argumentBinder;

        public ScriptBlockProcessor(IScriptBlockInfo scriptBlockInfo, CommandInfo commandInfo)
            : base(commandInfo)
        {
            _scriptBlockInfo = scriptBlockInfo;
            _fromFile = commandInfo.CommandType.Equals(CommandTypes.ExternalScript);
        }

        private void CreateOwnScope()
        {
            _originalContext = ExecutionContext;
            var executionSessionState = CommandInfo.Module != null ? CommandInfo.Module.SessionState
                                                                   : ExecutionContext.SessionState;
            _scopedContext = ExecutionContext.Clone(executionSessionState, _scriptBlockInfo.ScopeUsage);
            _scopedExecutionVisitor = new ExecutionVisitor(_scopedContext, CommandRuntime, false);
        }

        private void SwitchToOwnScope()
        {
            // globally for access through the runspace
            ExecutionContext.CurrentRunspace.ExecutionContext = _scopedContext;
            // privately for access through own reference
            ExecutionContext = _scopedContext;
        }

        private void RestoreOriginalScope()
        {
            ExecutionContext.CurrentRunspace.ExecutionContext = _originalContext;
            ExecutionContext = _originalContext;
        }

        /// <summary>
        /// Create the scope for this script block and binding arguments in it
        /// </summary>
        public override void Prepare()
        {
            // TODO: check if it makes sense to move the scope handling to the PipelineProcessor!
            //Let's see on the long run if there is an easier solution for this #ExecutionContextChange
            CreateOwnScope();
            SwitchToOwnScope();
            try
            {
                MergeParameters();
                _argumentBinder = new ScriptBlockParameterBinder(_scriptBlockInfo.GetParameters(), ExecutionContext,
                                                                 _scopedExecutionVisitor);
                _argumentBinder.BindCommandLineParameters(Parameters);
            }
            finally
            {
                RestoreOriginalScope();
            }
        }

        public override void BeginProcessing()
        {
            // nothing to do
            // TODO: process begin clause. remember to switch scope
        }

        /// <summary>
        /// In a script block, we only provide the pipeline as the automatic $input variable, but call it only once
        /// </summary>
        public override void ProcessRecords()
        {
            // TODO: provide the automatic $input variable
            // TODO: currently we don't support "cmdlet scripts", i.e. functions that behave like cmdlets.
            // TODO: Investigate the usage of different function blocks properly before implementing them
            //set the execution context of the runspace to the new context for execution in it
            SwitchToOwnScope();
            try
            {
                _scriptBlockInfo.ScriptBlock.Ast.Visit(_scopedExecutionVisitor);
            }
            catch (FlowControlException e)
            {
                if (!_fromFile || e is LoopFlowException)
                {
                    throw; // gets propagated if the script block is not an external script or it's a break/continue
                }
                if (e is ExitException)
                {
                    int exitCode = 0;
                    LanguagePrimitives.TryConvertTo<int>(((ExitException)e).Argument, out exitCode);
                    _exitCode = exitCode;
                    ExecutionContext.SetLastExitCodeVariable(exitCode);
                }
                // otherwise (return), we simply stop execution of the script (that's why we're here) and do nothing
            }
            finally //make sure we switch back to the original execution context, no matter what happened
            {
                RestoreOriginalScope();
            }
        }

        public override void EndProcessing()
        {
            // TODO: process end clause. remember to switch scope
            ExecutionContext.SetSuccessVariable(_exitCode == null || _exitCode == 0);
        }

        public override string ToString()
        {
            return this._scriptBlockInfo.ToString();
        }

        /// <summary>
        /// The parse currently adds each parameter without value and each value without parameter name, because
        /// it doesn't know at parse time whether the value belongs to the paramater or if the parameter is a switch
        /// parameter and the value is just a positional parameter. As we now know more abot the parameters, we can
        /// merge all parameters with the upcoming value if it's not a switch parameter
        /// </summary>
        void MergeParameters()
        {
            // This implementation has quite some differences to the definition for cmdlets.
            // For example "fun -a -b" can bind "-b" as value to parameter "-a" if "-b" doesn't exist.
            // This is different to cmdlet behavior.
            // Also, we will throw an error if we cannot merge a parameter, i.e. if only the name is defined: "fun -a"
            var definedParameterNames = (from param in _scriptBlockInfo.GetParameters()
                                         select param.Name.VariablePath.UserPath).ToList();
            var oldParameters = new Collection<CommandParameter>(new List<CommandParameter>(Parameters));
            Parameters.Clear();

            int numParams = oldParameters.Count;
            for (int i = 0; i < numParams; i++)
            {
                var current = oldParameters[i];
                var peek = (i < numParams - 1) ? oldParameters[i + 1] : null;

                // if we have a no name, or a value different to null (or explicitly null), just take it
                // otherwise it would have been merged before
                if (String.IsNullOrEmpty(current.Name) ||
                    current.Value != null ||
                    current.HasExplicitArgument)
                {
                    Parameters.Add(current);
                    continue;
                }

                // the current parameter has no argument (and not explicilty set to null), try to merge the next
                if (peek != null && String.IsNullOrEmpty(peek.Name))
                {
                    Parameters.Add(current.Name, peek.Value);
                    i++; //because of merge
                }
                // the next parameter might also be '-b' without "b" being a defined parameter, then we take it
                // as value
                else if (peek != null && peek.Value == null && !definedParameterNames.Contains(peek.Name))
                {
                    Parameters.Add(current.Name, peek.GetDecoratedName());
                    i++; //because of merge
                }
                else if (definedParameterNames.Contains(current.Name))
                {
                    // otherwise we don't have a value for this named parameter, throw an error
                    throw new ParameterBindingException("Missing argument for parameter '" + current.Name + "'");
                }
                else
                {
                    // this happens on an named parameter that is not part of the parameter definition
                    Parameters.Add(current);
                }
            }
        }

    }
}
