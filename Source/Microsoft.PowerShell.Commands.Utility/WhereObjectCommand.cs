// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Pash.Implementation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// Setting the default parameter set to be ScriptBlockSet. PowerShell has EqualSet as the default parameter
    /// but parameter binding in Pash binds the script block to the Property parameter when a script block is passed.
    /// </summary>
    [Cmdlet("Where", "Object", SupportsShouldProcess = true, DefaultParameterSetName = "ScriptBlockSet")]
    public sealed class WhereObjectCommand : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Parameter (Position = 0, Mandatory = true, ParameterSetName = "ScriptBlockSet")]
        public ScriptBlock FilterScript { get; set; }

        [ValidateNotNullOrEmpty]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "EqualSet")]
        public string Property { get; set; }

        [Parameter(Position = 1, ParameterSetName = "EqualSet")]
        public object Value { get; set; }

        [Parameter(ParameterSetName = "EqualSet")]
        [Alias ("IEQ")]
        public SwitchParameter EQ { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveContainsSet")]
        public SwitchParameter CContains { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveEqualSet")]
        public SwitchParameter CEQ { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveGreaterOrEqualSet")]
        public SwitchParameter CGE { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveGreaterThanSet")]
        public SwitchParameter CGT { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveInSet")]
        public SwitchParameter CIn { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveLessOrEqualSet")]
        public SwitchParameter CLE { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveLessThanSet")]
        public SwitchParameter CLT { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveLikeSet")]
        public SwitchParameter CLike { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveMatchSet")]
        public SwitchParameter CMatch { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveNotContainsSet")]
        public SwitchParameter CNotContains { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveNotEqualSet")]
        public SwitchParameter CNE { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveNotInSet")]
        public SwitchParameter CNotIn { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveNotLikeSet")]
        public SwitchParameter CNotLike { get; set; }

        [Parameter(ParameterSetName = "CaseSensitiveNotMatchSet")]
        public SwitchParameter CNotMatch { get; set; }

        [Parameter(ParameterSetName = "ContainsSet")]
        public SwitchParameter Contains { get; set; }

        [Parameter(ParameterSetName = "GreaterOrEqualSet")]
        public SwitchParameter GE { get; set; }

        [Parameter(ParameterSetName = "GreaterThanSet")]
        public SwitchParameter GT { get; set; }

        [Parameter(ParameterSetName = "InSet")]
        public SwitchParameter In { get; set; }

        [Parameter(ParameterSetName = "IsNotSet")]
        public SwitchParameter IsNot { get; set; }

        [Parameter(ParameterSetName = "IsSet")]
        public SwitchParameter Is { get; set; }

        [Parameter(ParameterSetName = "LessOrEqualSet")]
        public SwitchParameter LE { get; set; }

        [Parameter(ParameterSetName = "LessThanSet")]
        public SwitchParameter LT { get; set; }

        [Parameter(ParameterSetName = "LikeSet")]
        public SwitchParameter Like { get; set; }

        [Parameter(ParameterSetName = "MatchSet")]
        public SwitchParameter Match { get; set; }

        [Parameter(ParameterSetName = "NotContainsSet")]
        public SwitchParameter NotContains { get; set; }

        [Parameter(ParameterSetName = "NotEqualSet")]
        public SwitchParameter NE { get; set; }

        [Parameter(ParameterSetName = "NotInSet")]
        public SwitchParameter NotIn { get; set; }

        [Parameter(ParameterSetName = "NotLikeSet")]
        public SwitchParameter NotLike { get; set; }

        [Parameter(ParameterSetName = "NotMatchSet")]
        public SwitchParameter NotMatch { get; set; }

        protected override void ProcessRecord()
        {
            if (FilterScript != null)
            {
                FilterByScript();
            }
            else
            {
                FilterByProperty();
            }
        }

        private void FilterByScript()
        {
            ExecutionContext.SetVariable("_", InputObject);

            var executionVisitor = new ExecutionVisitor(ExecutionContext, (PipelineCommandRuntime)CommandRuntime);

            object result = executionVisitor.EvaluateAst(FilterScript.Ast, true);
            if (IsTrue(result))
            {
                WriteObject(InputObject);
            }
        }

        private bool IsTrue(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            PSObject result = PSObject.AsPSObject(obj);
            if (result.ImmediateBaseObject is bool)
            {
                return (bool)result.ImmediateBaseObject;
            }
            return false;
        }

        private void FilterByProperty()
        {
            object propertyValue = GetPropertyValue();

            if (ValuePassesFilter(propertyValue))
            {
                WriteObject(InputObject);
            }
        }

        private object GetPropertyValue()
        {
            if (InputObject == null)
            {
                return null;
            }

            PSPropertyInfo inputObjectProperty = InputObject.Properties[Property];
            if (inputObjectProperty != null)
            {
                return inputObjectProperty.Value;
            }

            return null;
        }

        private bool ValuePassesFilter(object propertyValue)
        {
            if (EQ.IsPresent)
            {
                return LanguagePrimitives.Equals(propertyValue, Value, true);
            }
            throw new NotImplementedException("Where-Object filter not implemented");
        }
    }
}
