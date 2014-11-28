using System;
using System.Management.Automation.Language;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace System.Management.Pash.Implementation
{
    public class SettableVariableExpression : SettableExpression
    {
        private VariableExpressionAst _expressionAst;

        internal SettableVariableExpression(VariableExpressionAst expressionAst, ExecutionVisitor currentExecution)
            : base(currentExecution)
        {
            _expressionAst = expressionAst;
        }

        public override object GetValue()
        {
            if (_expressionAst.VariablePath.IsDriveQualified)
            {
                return GetDriveQualifiedVariableValue();
            }

            var variable = GetUnqualifiedVariable();
            var value = (variable != null) ? variable.Value : null;
            return value;
        }

        public override void SetValue(object value)
        {
            if (_expressionAst.VariablePath.IsDriveQualified)
            {
                SetDriveQualifiedVariableValue(value);
            }
            else
            {
                CurrentExecution.ExecutionContext.SetVariable(_expressionAst.VariablePath.UserPath, value);
            }
        }

        private PSVariable GetUnqualifiedVariable()
        {
            var variableIntrinsics = CurrentExecution.ExecutionContext.SessionState.PSVariable;
            return variableIntrinsics.Get(_expressionAst.VariablePath.UserPath);
        }

        private object GetDriveQualifiedVariableValue()
        {
            SessionStateProviderBase provider = GetSessionStateProvider(_expressionAst.VariablePath);
            if (provider == null)
            {
                return null;
            }
            var path = new Path(_expressionAst.VariablePath.GetUnqualifiedUserPath());
            object item = provider.GetSessionStateItem(path);
            return item == null ? null : provider.GetValueOfItem(item);
        }

        private void SetDriveQualifiedVariableValue(object value)
        {
            SessionStateProviderBase provider = GetSessionStateProvider(_expressionAst.VariablePath);
            if (provider != null)
            {
                var path = new Path(_expressionAst.VariablePath.GetUnqualifiedUserPath());
                provider.SetSessionStateItem(path, value, false);
            }
        }

        private SessionStateProviderBase GetSessionStateProvider(VariablePath variablePath)
        {
            PSDriveInfo driveInfo;
            if (!CurrentExecution.ExecutionContext.SessionState.Drive.TryGet(variablePath.DriveName, out driveInfo))
            {
                return null;
            }
            return CurrentExecution.ExecutionContext.SessionStateGlobal.GetProviderInstance(driveInfo.Provider.Name)
                as SessionStateProviderBase;
        }
    }
}

