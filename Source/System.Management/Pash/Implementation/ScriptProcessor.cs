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
    internal class ScriptProcessor : CommandProcessorBase
    {
        readonly ScriptInfo _scriptInfo;


        public ScriptProcessor(ScriptInfo scriptInfo)
            : base(scriptInfo)
        {
            this._scriptInfo = scriptInfo;
        }

        internal override ICommandRuntime CommandRuntime { get; set; }

        internal override void BindArguments(PSObject obj)
        {
            ReadOnlyCollection<ParameterAst> scriptParameters = _scriptInfo.GetParameters();

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
            return GetDefaultParameterValue(scriptParameter);
        }

        private CommandParameter GetParameterByPosition(int position)
        {
            if (Parameters.Count > position)
            {
                return Parameters[position];
            }
            return null;
        }

        private object GetDefaultParameterValue(ParameterAst scriptParameter)
        {
            var constantExpression = scriptParameter.DefaultValue as ConstantExpressionAst;
            if (constantExpression != null)
            {
                return constantExpression.Value;
            }
            return null;
        }

        internal override void Initialize()
        {
            // TODO: initialize ScriptProcessor
        }

        internal override void ProcessRecord()
        {
            ExecutionContext context = ExecutionContext.Clone();

            PipelineCommandRuntime pipelineCommandRuntime = (PipelineCommandRuntime)CommandRuntime;

            this._scriptInfo.ScriptBlock.Ast.Visit(new ExecutionVisitor(context, pipelineCommandRuntime, false));
        }

        internal override void Complete()
        {
            // TODO: do a full cleanup
        }

        public override string ToString()
        {
            return this._scriptInfo.ToString();
        }
    }
}
