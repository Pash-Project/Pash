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


        public ScriptBlockProcessor(IScriptBlockInfo scriptBlockInfo, CommandInfo commandInfo)
            : base(commandInfo)
        {
            _scriptBlockInfo = scriptBlockInfo;
            _fromFile = commandInfo.CommandType.Equals(CommandTypes.ExternalScript);
        }

        private void BindArguments()
        {
            ReadOnlyCollection<ParameterAst> scriptParameters = _scriptBlockInfo.GetParameters();

            // iterate over all provided parameters and bind the args to them
            for (int i = 0; i < scriptParameters.Count; ++i)
            {
                ParameterAst scriptParameter = scriptParameters[i];
                CommandParameter parameter = GetParameterByPosition(i);
                object parameterValue = GetParameterValue(scriptParameter, parameter);

                ExecutionContext.SetVariable(scriptParameter.Name.VariablePath.UserPath, parameterValue);
            }
        }

        private object GetParameterValue(ParameterAst scriptParameter, CommandParameter parameter)
        {
            if (parameter != null)
            {
                return parameter.Value;
            }

            if (scriptParameter.DefaultValue != null)
            {
                return _scopedExecutionVisitor.EvaluateAst(scriptParameter.DefaultValue);
            }
            return null;
        }

        private CommandParameter GetParameterByPosition(int position)
        {
            if (Parameters.Count > position)
            {
                return Parameters[position];
            }
            return null;
        }

        private void CreateOwnScope()
        {
            _originalContext = ExecutionContext;
            _scopedContext = ExecutionContext.Clone(_scriptBlockInfo.ScopeUsage);
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
                BindArguments();
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
                    ExecutionContext.SetVariable("global:LASTEXITCODE", exitCode);
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
            ExecutionContext.SetVariable("global:?", _exitCode == null || _exitCode == 0);
        }

        public override string ToString()
        {
            return this._scriptBlockInfo.ToString();
        }
    }
}
