// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.IO;
using NUnit.Framework;

namespace TestHost
{
    [TestFixture]
    public class DotSourceTests
    {
        [TearDown]
        public void RemoveScriptFile()
        {
            File.Delete(GetScriptFileName());
        }

        private string GetScriptFileName()
        {
            string directory = Path.GetDirectoryName(typeof(DotSourceTests).Assembly.Location);
            return Path.Combine(directory, "DotSourceTests.ps1");
        }

        private string CreateScript(string script)
        {
            string fileName = GetScriptFileName();
            File.WriteAllText(fileName, script);

            return fileName;
        }

        [Test]
        public void DotSourceScriptUsingFullFileNameInQuotes()
        {
            string fileName = CreateScript(@"Write-Host 'script output'");
            string statement = string.Format(". '{0}'", fileName);

            string result = TestHost.Execute(statement);

            Assert.AreEqual(result, string.Format("script output{0}", Environment.NewLine));
        }

        [Test]
        public void DotSourceScriptUsingFullFileName()
        {
            string fileName = CreateScript(@"Write-Host 'script output'");
            string statement = string.Format(". {0}", fileName);

            string result = TestHost.Execute(statement);

            Assert.AreEqual(result, string.Format("script output{0}", Environment.NewLine));
        }

        [Test]
        public void VariableDefinedInDotSourcedScriptIsAvailableAfterDotSourcing()
        {
            string fileName = CreateScript(@"$test = 'variable value'");

            string result = TestHost.Execute(
                string.Format(". '{0}'", fileName),
                "Write-Host $test");

            Assert.AreEqual(result, string.Format("variable value{0}", Environment.NewLine));
        }

        [Test]
        public void VariableDefinedBeforeDotSourceIsAvailableInDotSourcedScript()
        {
            string fileName = CreateScript(@"Write-Host $test");

            string result = TestHost.Execute(
                "$test = 'variable value'",
                string.Format(". '{0}'", fileName));

            Assert.AreEqual(result, string.Format("variable value{0}", Environment.NewLine));
        }

        [Test]
        public void DotSourceScriptWithFileNameTakenFromVariable()
        {
            string fileName = CreateScript(@"Write-Host 'script output'");

            string result = TestHost.Execute(
                string.Format("$fileName = '{0}'", fileName),
                ". $fileName");

            Assert.AreEqual(result, string.Format("script output{0}", Environment.NewLine));
        }

        [Test]
        public void DotSourceScriptWithFileNameTakenFromJoinPath()
        {
            string fullPath = CreateScript(@"Write-Host 'script output'");
            string directory = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileName(fullPath);

            string result = TestHost.Execute(
                string.Format("$directory = '{0}'", directory),
                string.Format("$fileName = '{0}'", fileName),
                ". (Join-Path $directory $fileName)");

            Assert.AreEqual(result, string.Format("script output{0}", Environment.NewLine));
        }

        [Test]
        public void DotSourceScriptBlockVariableTest()
        {
            string statement = "$a=0; Write-Host $a; . { $a = 10; Write-Host $a }; Write-Host $a;";
            string result = TestHost.Execute(statement);
            string expected = String.Join(Environment.NewLine, new string[] { "0", "10", "10" }) + Environment.NewLine;
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void DotSourceVariableScriptBlockVariableTest()
        {
            string statement = "$a=0; Write-Host $a; $b = { $a = 10; Write-Host $a }; . $b; Write-Host $a;";
            string result = TestHost.Execute(statement);
            string expected = String.Join(Environment.NewLine, new string[] { "0", "10", "10" }) + Environment.NewLine;
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void DotSourceOverwriteFunctionTest()
        {
            string fileName = CreateScript("function foo { Write-Host 'bar' };");
            string result = TestHost.Execute(
                "function foo { Write-Host 'foo' }",
                "foo;", //prints "foo"
                string.Format("$fileName = '{0}'", fileName),
                ". $fileName",
                "foo;" //was overwritten and prints now "bar"
            );
            string expected = String.Join(Environment.NewLine, new string[] { "foo", "bar" }) + Environment.NewLine;

            Assert.AreEqual(expected, result);
        }
    }
}
