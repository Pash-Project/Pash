// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class SetContentTests : ReferenceTestBase
    {
        private string _tempFileName;

        private string GenerateTempFile(string content, string fileName = "content.txt")
        {
            _tempFileName = Path.Combine(Path.GetTempPath(), fileName);
            File.WriteAllText(_tempFileName, content);
            AddCleanupFile(_tempFileName);
            return _tempFileName;
        }

        [Test]
        public void StringValueOverwritesExistingFile()
        {
            string fileName = GenerateTempFile("test");

            string result = ReferenceHost.Execute(new string[] {
                "Set-Content -value 'abc' -path " + fileName,
                "Get-Content " + fileName
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }

        [Test]
        public void NoNamedParameters()
        {
            string fileName = GenerateTempFile("test");

            string result = ReferenceHost.Execute(new string[] {
                "Set-Content " + fileName + " 'abc'",
                "Get-Content " + fileName
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }

        [Test]
        public void ValueTakenFromPipeline()
        {
            string fileName = GenerateTempFile("test");

            string result = ReferenceHost.Execute(new string[] {
                "'abc' | Set-Content " + fileName,
                "Get-Content " + fileName
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }

        [Test]
        public void TwoFiles()
        {
            string fileName1 = GenerateTempFile("1", "input1.txt");
            string fileName2 = GenerateTempFile("2", "input2.txt");

            string result = ReferenceHost.Execute("'abc' | Set-Content " + fileName1 + "," + fileName2);

            string input1Text = File.ReadAllText(fileName1);
            string input2Text = File.ReadAllText(fileName2);
            Assert.AreEqual("abc" + Environment.NewLine, input1Text);
            Assert.AreEqual("abc" + Environment.NewLine, input2Text);
        }

        [Test, Explicit("Array values passed one at a time to Set-Content and not as an array")]
        public void ValueArrayTakenFromPipeline()
        {
            string fileName = GenerateTempFile("test");

            string result = ReferenceHost.Execute(new string[] {
                "'abc','def' | Set-Content " + fileName,
                "Get-Content " + fileName
            });

            Assert.AreEqual(NewlineJoin("abc", "def"), result);
        }

        [Test]
        public void ArrayOfValues()
        {
            string fileName = GenerateTempFile("test");

            string result = ReferenceHost.Execute(new string[] {
                "$a = 'abc','def'",
                "Set-Content -value $a -path " + fileName,
                "Get-Content " + fileName
            });

            Assert.AreEqual(NewlineJoin("abc", "def"), result);
        }

        [Test]
        public void FileDoesNotExist()
        {
            string fileName = GenerateTempFile("test");
            File.Delete(fileName);

            string result = ReferenceHost.Execute(new string[] {
                "Set-Content -value 'abc' -path " + fileName,
                "Get-Content " + fileName
            });

            Assert.AreEqual("abc" + Environment.NewLine, result);
        }
    }
}
