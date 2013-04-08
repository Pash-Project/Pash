// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
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
        public new string Value { get; private set; }

        public override string ToString()
        {
            switch (this.StringConstantType)
            {
                case StringConstantType.SingleQuoted:
                    return string.Format("'{0}'", this.Value);

                case StringConstantType.SingleQuotedHereString:
                    return string.Format("@'{0}'", this.Value);

                case StringConstantType.DoubleQuoted:
                    return string.Format("\"{0}\"", this.Value);

                case StringConstantType.DoubleQuotedHereString:
                    return string.Format("@\"{0}\"", this.Value);

                case StringConstantType.BareWord:
                    return string.Format("{0}", this.Value);


                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
