using System;
using System.Collections.Generic;

namespace System.Management.Automation.Language
{
    public class StringConstantExpressionAst : ConstantExpressionAst
    {
        public StringConstantExpressionAst(IScriptExtent extent, string value, StringConstantType stringConstantType)
            : base(extent, value)
        {
            this.StringConstantType = stringConstantType;
            this.Value = value;
        }

        public override Type StaticType { get { return typeof(string); } }
        public StringConstantType StringConstantType { get; private set; }
        public string Value { get; private set; }
    }
}
