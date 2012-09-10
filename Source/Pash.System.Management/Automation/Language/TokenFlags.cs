using System;

namespace System.Management.Automation.Language
{
    [Flags]
    public enum TokenFlags
    {
        // TODO: Rewrite these as hex values. Fun.
        None = 0,
        BinaryPrecedenceLogical = 1,
        BinaryPrecedenceBitwise = 2,
        BinaryPrecedenceComparison = 3,
        BinaryPrecedenceAdd = 4,
        BinaryPrecedenceMultiply = 5,
        BinaryPrecedenceFormat = 6,
        BinaryPrecedenceMask = 7,
        BinaryPrecedenceRange = 7,
        Keyword = 16,
        ScriptBlockBlockName = 32,
        BinaryOperator = 256,
        UnaryOperator = 512,
        CaseSensitiveOperator = 1024,
        SpecialOperator = 4096,
        AssignmentOperator = 8192,
        ParseModeInvariant = 32768,
        TokenInError = 65536,
        DisallowedInRestrictedMode = 131072,
        PrefixOrPostfixOperator = 262144,
        CommandName = 524288,
        MemberName = 1048576,
        TypeName = 2097152,
        AttributeName = 4194304,
        CanConstantFold = 8388608,
    }
}
