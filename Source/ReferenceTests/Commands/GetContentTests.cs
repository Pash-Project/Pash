// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class GetContentTests : ReferenceTestBase
    {
        private string _tempFileName;

        private string GenerateTempFile(string content, string fileName = "input.txt")
        {
            _tempFileName = Path.Combine(Path.GetTempPath(), fileName);
            File.WriteAllText(_tempFileName, content);
            AddCleanupFile(_tempFileName);
            return _tempFileName;
        }

        [Test]
        public void TextFileWithTwoLines()
        {
            string fileName = GenerateTempFile(NewlineJoin("1", "2"));

            string result = ReferenceHost.Execute("Get-Content " + fileName);

            Assert.AreEqual(NewlineJoin("1", "2"), result);
        }

        [Test]
        public void TextFileIsIntoObjectArrayContainingStrings()
        {
            string fileName = GenerateTempFile(NewlineJoin("1", "2", "3"));

            string result = ReferenceHost.Execute(new string[] {
                "$c = Get-Content -path " + fileName,
                "$array = $c.GetType().Name",
                "$item = $c[0].GetType().Name",
                "\"$array - $item\""
            });

            Assert.AreEqual("Object[] - String" + Environment.NewLine, result);
        }

        [Test]
        public void ReadTwoFiles()
        {
            string fileName1 = GenerateTempFile(NewlineJoin("1", "2"), "input1.txt");
            string fileName2 = GenerateTempFile(NewlineJoin("3", "4"), "input2.txt");

            string result = ReferenceHost.Execute("Get-Content " + fileName1 + "," + fileName2);

            Assert.AreEqual(NewlineJoin("1", "2", "3", "4"), result);
        }

        [Test]
        public void ReadFirstTwoLinesOfThreeLineTextFile()
        {
            string fileName = GenerateTempFile(NewlineJoin("1", "2", "3"));

            string result = ReferenceHost.Execute("Get-Content -totalcount 2 " + fileName);

            Assert.AreEqual(NewlineJoin("1", "2"), result);
        }

        [Test]
        public void ReadFirstTwoLinesOfThreeLineTextFileUsingFirstParameterAlias()
        {
            string fileName = GenerateTempFile(NewlineJoin("1", "2", "3"));

            string result = ReferenceHost.Execute("Get-Content -first 2 " + fileName);

            Assert.AreEqual(NewlineJoin("1", "2"), result);
        }

        [Test]
        public void ReadFirstTwoLinesOfThreeLineTextFileUsingHeadParameterAlias()
        {
            string fileName = GenerateTempFile(NewlineJoin("1", "2", "3"));

            string result = ReferenceHost.Execute("Get-Content -head 2 " + fileName);

            Assert.AreEqual(NewlineJoin("1", "2"), result);
        }

        [Test]
        public void ReadNoLinesOfFileThatDoesNotExist()
        {
            string result = ReferenceHost.Execute("Get-Content -first 0 unknownfile.txt");

            Assert.AreEqual(String.Empty, result);
        }

        [Test]
        public void ReadLastTwoLinesOfThreeLineTextFile()
        {
            string fileName = GenerateTempFile(NewlineJoin("1", "2", "3"));

            string result = ReferenceHost.Execute("Get-Content -tail 2 " + fileName);

            Assert.AreEqual(NewlineJoin("2", "3"), result);
        }

        [Test]
        public void ReadLastTwoLinesOfThreeLineTextFileUsingLastParameterAlias()
        {
            string fileName = GenerateTempFile(NewlineJoin("1", "2", "3"));

            string result = ReferenceHost.Execute("Get-Content -last 2 " + fileName);

            Assert.AreEqual(NewlineJoin("2", "3"), result);
        }

        [Test]
        public void ReadTwoLinesInOneGoUsingReadCount()
        {
            string fileName = GenerateTempFile(NewlineJoin("A1", "B2", "C3", "D4"));

            string result = ReferenceHost.Execute(new string[] {
                "$items = Get-Content -ReadCount 2 -path " + fileName,
                "$a = $items[0][0]",
                "$b = $items[0][1]",
                "$c = $items[1][0]",
                "$d = $items[1][1]",
                "\"$a $b $c $d\""
            });

            Assert.AreEqual("A1 B2 C3 D4" + Environment.NewLine, result);
        }

        [Test]
        public void TypesUsedWhenReadTwoLinesInOneGoUsingReadCount()
        {
            string fileName = GenerateTempFile(NewlineJoin("1", "2", "3", "4"));

            string result = ReferenceHost.Execute(new string[] {
                "$c = Get-Content -ReadCount 2 -path " + fileName,
                "$array = $c.GetType().Name",
                "$item = $c[0].GetType().Name",
                "$item2 = $c[0][0].GetType().Name",
                "\"$array - $item - $item2\""
            });

            Assert.AreEqual("Object[] - Object[] - String" + Environment.NewLine, result);
        }

        [Test]
        public void ReadTwoLinesInOneGoUsingReadCountButOnlyThreeLinesInTotal()
        {
            string fileName = GenerateTempFile(NewlineJoin("A1", "B2", "C3"));

            string result = ReferenceHost.Execute(new string[] {
                "$items = Get-Content -ReadCount 2 -path " + fileName,
                "$a = $items[0][0]",
                "$b = $items[0][1]",
                "$c = $items[1][0]",
                "\"$a $b $c\""
            });

            Assert.AreEqual("A1 B2 C3" + Environment.NewLine, result);
        }

        [Test]
        public void ReadTwoLinesInOneGoUsingReadCountButReadTheLastThreeLines()
        {
            string fileName = GenerateTempFile(NewlineJoin("A1", "B2", "C3", "D4"));

            string result = ReferenceHost.Execute(new string[] {
                "$items = Get-Content -ReadCount 2 -Tail 3 -path " + fileName,
                "$a = $items[0][0]",
                "$b = $items[0][1]",
                "$c = $items[1][0]",
                "\"$a $b $c\""
            });

            Assert.AreEqual("B2 C3 D4" + Environment.NewLine, result);
        }
    }
}
