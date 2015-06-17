using System;
using NUnit.Framework;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class ExpressionTests : ReferenceTestBase
    {

        // a void value is not written to pipeline (see GeneralConversionTest.ConvertToVoidNotWrittenToPipeline)
        // but it can be used in an expression as null. Here are just some examples
        [TestCase("[string]::IsNullOrEmpty([void]0)")]
        [TestCase("[void]'foo' -eq $null")]
        [TestCase("$a = [void]'foo'; $var = Get-Variable a; ($var.name -eq 'a') -and ($var.value -eq $null)")]
        public void EmptyPipeExpressionIsHandledAsNull(string cmd)
        {
            ExecuteAndCompareTypedResult(cmd, true);
        }
    }
}

