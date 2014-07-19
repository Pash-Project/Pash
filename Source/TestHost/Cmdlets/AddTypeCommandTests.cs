// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace TestHost.Cmdlets
{
    /// <summary>
    /// These tests should be moved to ReferenceTests once the $error and pipeline.Errors problems
    /// have been fixed:
    /// 
    /// 1) When an exception is thrown in a cmdlet all the errors written to the error stream
    /// are not displayed in the console. Windows PowerShell will display the exception and all the
    /// error records written to the error stream.
    /// 2) Duplicate error records are added to $error when a cmdlet throws an exception.
    /// </summary>
    [TestFixture]
    public class AddTypeCommandTests
    {
        string tempFileName;

        [TearDown]
        public void RemoveTempFile()
        {
            if (tempFileName != null && File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
        }

        private string CreateTempFile(string fileName, string contents)
        {
            string directory = Path.GetTempPath();
            tempFileName = Path.Combine(Path.GetTempPath(), fileName);
            File.WriteAllText(tempFileName, contents);
            return tempFileName;
        }

        [Test]
        public void AddTypeDefinitionWithInvalidCSharpCode()
        {
            string result = TestHost.ExecuteWithZeroErrors(
                "Add-Type -TypeDefinition 'public class ErrorTest --'",
                "'error[0].Exception.Message=' + $error[0].Exception.Message",
                "'error[0].FullyQualifiedErrorId=' + $error[0].FullyQualifiedErrorId",
                "$compilerErrorCount = 0",
                "$error | ForEach-Object { if ( $_.FullyQualifiedErrorId -eq 'SOURCE_CODE_ERROR,Microsoft.PowerShell.Commands.AddTypeCommand') { $compilerErrorCount += 1 } }",
                "if ($compilerErrorCount -gt 0) { 'Compiler error reported' }"
            );

            StringAssert.Contains("error[0].Exception.Message=Cannot add type. There were compilation errors.", result);
            // TODO: Original error record information is lost.
            //StringAssert.Contains("error[0].FullyQualifiedErrorId=COMPILER_ERRORS,Microsoft.PowerShell.Commands.AddTypeCommand", result);
            StringAssert.Contains("Compiler error reported", result);
        }

        [Test]
        public void AddMemberDefinitionWithInvalidCSharp()
        {
            string result = TestHost.ExecuteWithZeroErrors(
                "add-type -name Test -memberdefinition 'public WriteLine() ---'",
                "'error[0].Exception.Message=' + $error[0].Exception.Message",
                "'error[0].FullyQualifiedErrorId=' + $error[0].FullyQualifiedErrorId",
                "$compilerErrorCount = 0",
                "$error | ForEach-Object { if ( $_.FullyQualifiedErrorId -eq 'SOURCE_CODE_ERROR,Microsoft.PowerShell.Commands.AddTypeCommand') { $compilerErrorCount += 1 } }",
                "if ($compilerErrorCount -gt 0) { 'Multiple Compiler Errors' }"
            );

            StringAssert.Contains("error[0].Exception.Message=Cannot add type. There were compilation errors.", result);
            // TODO: Original error record information is lost.
            //StringAssert.Contains("error[0].FullyQualifiedErrorId=COMPILER_ERRORS,Microsoft.PowerShell.Commands.AddTypeCommand", result);
            StringAssert.Contains("Multiple Compiler Errors", result);
        }

        [Test]
        public void AddTypeDefinitionFromFileWithInvalidCSharpCode()
        {
            string fileName = CreateTempFile("AddTypeDefinitionFromFileWithInvalidCSharpCode.cs", "public class ErrorTest --");
            string result = TestHost.ExecuteWithZeroErrors(
                "Add-Type -Path '" + fileName + "'",
                "'error[0].Exception.Message=' + $error[0].Exception.Message",
                "'error[0].FullyQualifiedErrorId=' + $error[0].FullyQualifiedErrorId",
                "$compilerErrorCount = 0",
                "$error | ForEach-Object { if ( $_.FullyQualifiedErrorId -eq 'SOURCE_CODE_ERROR,Microsoft.PowerShell.Commands.AddTypeCommand') { $compilerErrorCount += 1 } }",
                "if ($compilerErrorCount -gt 0) { 'Compiler error reported' }"
            );

            StringAssert.Contains("error[0].Exception.Message=Cannot add type. There were compilation errors.", result);
            // TODO: Original error record information is lost.
            //StringAssert.Contains("error[0].FullyQualifiedErrorId=COMPILER_ERRORS,Microsoft.PowerShell.Commands.AddTypeCommand", result);
            StringAssert.Contains("Compiler error reported", result);
        }
    }
}
