using System;

namespace System.Management.Automation.Language
{
    public enum StringConstantType
    {
        SingleQuoted = 0,
        SingleQuotedHereString = 1,
        DoubleQuoted = 2,
        DoubleQuotedHereString = 3,
        BareWord = 4,
    }
}
