// Copyright (C) Pash Contributors. License GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using NUnit.Framework;
using System.IO;

namespace TestHost
{
    [TestFixture]
    public class ScriptParameterTests
    {
        [TearDown]
        public void RemoveScriptFile()
        {
            File.Delete(GetScriptFileName());
        }

        private string GetScriptFileName()
        {
            string directory = Path.GetDirectoryName(typeof(ScriptParameterTests).Assembly.Location);
            return Path.Combine(directory, "ScriptParameterTests.ps1");
        }

        private string CreateScript(string script)
        {
            string fileName = GetScriptFileName();
            File.WriteAllText(fileName, script);

            return fileName;
        }

        [Test]
        public void SingleParameterPassedToScriptThatTakesOneParameterCanAccessParameterInScript()
        {
            string fileName = CreateScript(@"param($param1) Write-Host $param1");
            string statement = string.Format("& '{0}' 'test'", fileName);

            string result = TestHost.Execute(statement);

            Assert.AreEqual(result, string.Format("test{0}", Environment.NewLine));
        }

        [Test, Explicit("Currently only finds param statement on first line")]
        public void SingleParameterPassedToScriptWithParamStatementNotOnFirstLineCanAccessParameterInScript()
        {
            string fileName = CreateScript(@"
param($param1)
Write-Host $param1");
            string statement = string.Format("& '{0}' 'test'", fileName);

            string result = TestHost.Execute(statement);

            Assert.AreEqual(result, string.Format("test{0}", Environment.NewLine));
        }

        [Test]
        public void NoParametersPassedToScriptThatTakesOneParameterWithDefaultConstantValueCausesDefaultValueToBePassedAsParameter()
        {
            string fileName = CreateScript(@"param($param1='defaultValue') Write-Host $param1");
            string statement = string.Format("& '{0}'", fileName);

            string result = TestHost.Execute(statement);

            Assert.AreEqual(result, string.Format("defaultValue{0}", Environment.NewLine));
        }
    }
}
