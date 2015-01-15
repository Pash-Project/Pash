using System;
using System.Management.Automation.Language;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace System.Management.Pash.Implementation
{
    public class SettableVariableExpression : SettableExpression
    {
        private VariableExpressionAst _expressionAst;

        public VariablePath VariablePath {
            get
            {
                return _expressionAst.VariablePath;
            }
        }

        internal SettableVariableExpression(VariableExpressionAst expressionAst, ExecutionVisitor currentExecution)
            : base(currentExecution)
        {
            _expressionAst = expressionAst;
        }

        public override object GetValue()
        {
            if (VariablePath.IsDriveQualified)
            {
                return GetDriveQualifiedVariableValue();
            }

            var variable = GetUnqualifiedVariable();
            var value = (variable != null) ? variable.Value : null;
            return value;
        }

        public override void SetValue(object value)
        {
            if (VariablePath.IsDriveQualified)
            {
                SetDriveQualifiedVariableValue(value);
            }
            else
            {
                ExecutionContext.SetVariable(VariablePath.UserPath, value);
            }
        }

        private PSVariable GetUnqualifiedVariable()
        {
            var variableIntrinsics = ExecutionContext.SessionState.PSVariable;
            return variableIntrinsics.Get(VariablePath.UserPath);
        }

        private object GetDriveQualifiedVariableValue()
        {
            SessionStateProviderBase provider = GetSessionStateProvider(VariablePath);
            if (provider == null)
            {
                return null;
            }
            var path = new Path(VariablePath.GetUnqualifiedUserPath());
            object item = provider.GetSessionStateItem(path);
            return item == null ? null : provider.GetValueOfItem(item);
        }

        private void SetDriveQualifiedVariableValue(object value)
        {
            SessionStateProviderBase provider = GetSessionStateProvider(VariablePath);
            if (provider != null)
            {
                var path = new Path(VariablePath.GetUnqualifiedUserPath());
                provider.SetSessionStateItem(path, value, false);
            }
        }

        private SessionStateProviderBase GetSessionStateProvider(VariablePath variablePath)
        {
            PSDriveInfo driveInfo;
            if (!ExecutionContext.SessionState.Drive.TryGet(variablePath.DriveName, out driveInfo))
            {
                return null;
            }
            return ExecutionContext.SessionState.Provider.GetInstance(driveInfo.Provider.Name)
                as SessionStateProviderBase;
        }
    }
}

