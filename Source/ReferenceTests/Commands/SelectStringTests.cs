// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Microsoft.PowerShell.Commands;
using System.Text.RegularExpressions;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class SelectStringTests : ReferenceTestBase
    {
        [Test]
        public void FileWithSingleLineAndPatternMatchesTextInLine()
        {
            string fileName = CreateFile("first line", ".txt");

            string command = string.Format("Select-String -Path '{0}' -Pattern 'first'", fileName);
            MatchInfo match = ReferenceHost.RawExecute(command)
                .Select(psObject => (MatchInfo)psObject.ImmediateBaseObject)
                .Single();
            Match regexMatch = match.Matches.FirstOrDefault();

            Assert.IsNull(match.Context);
            Assert.AreEqual(1, match.LineNumber);
            Assert.AreEqual("first line", match.Line);
            Assert.AreEqual(Path.GetFileName(fileName), match.Filename);
            Assert.IsTrue(match.IgnoreCase);
            Assert.AreEqual("first", match.Pattern);
            Assert.AreEqual(fileName, match.Path);
            Assert.AreEqual(1, match.Matches.Length);
            Assert.IsTrue(regexMatch.Success);
            Assert.AreEqual(0, regexMatch.Index);
            Assert.AreEqual(5, regexMatch.Length);
            Assert.AreEqual("first", regexMatch.Value);
        }

        [Test]
        public void MatchInfoToStringShowsFileNameLineNumberAndLineText()
        {
            string fileName = CreateFile("first line", ".txt");

            string command = string.Format("(sls -Path '{0}' -Pattern 'first').ToString()", fileName);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual(fileName + ":1:first line" + Environment.NewLine, result);
        }

        [Test]
        public void NoMatchesInFile()
        {
            string fileName = CreateFile("first line", ".txt");

            string command = string.Format("(Select-string -Path '{0}' -Pattern 'notfound') -eq $null", fileName);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual("True" + Environment.NewLine, result);
        }

        [Test]
        public void SimpleMatchWhenPathFirstParameterAndPatternSecondAsNamedParameter()
        {
            string fileName = CreateFile("first line", ".txt");

            string command = string.Format("(Select-String '{0}' -Pattern 'first').ToString()", fileName);
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual(fileName + ":1:first line" + Environment.NewLine, result);
        }

        [Test]
        public void FileWithPatternMatchesTextOnTwoLines()
        {
            string fileName = CreateFile(NewlineJoin("first line", "second line"), ".txt");

            string command = string.Format("Select-String 'line' '{0}'", fileName);
            MatchInfo[] matches = ReferenceHost.RawExecute(command)
                .Select(psObject => (MatchInfo)psObject.ImmediateBaseObject)
                .ToArray();
            MatchInfo firstMatch = matches[0];
            MatchInfo secondMatch = matches[1];

            Assert.AreEqual(2, matches.Length);
            Assert.AreEqual(1, firstMatch.LineNumber);
            Assert.AreEqual(2, secondMatch.LineNumber);
            Assert.AreEqual("first line", firstMatch.Line);
            Assert.AreEqual("second line", secondMatch.Line);
            Assert.AreEqual(Path.GetFileName(fileName), secondMatch.Filename);
            Assert.AreEqual("line", secondMatch.Pattern);
            Assert.AreEqual(fileName, secondMatch.Path);
            Assert.AreEqual(6, firstMatch.Matches.Single().Index);
            Assert.AreEqual(4, firstMatch.Matches.Single().Length);
            Assert.AreEqual(7, secondMatch.Matches.Single().Index);
            Assert.AreEqual(4, secondMatch.Matches.Single().Length);
        }

        [Test]
        public void TwoPatternsPassedAsParameterFirstPatternMatchesFirstLineSecondPatternMatchesSecondLine()
        {
            string fileName = CreateFile(NewlineJoin("first line", "second line"), ".txt");

            string command = string.Format("Select-String -Path '{0}' -Pattern 'first','line'", fileName);
            MatchInfo[] matches = ReferenceHost.RawExecute(command)
                .Select(psObject => (MatchInfo)psObject.ImmediateBaseObject)
                .ToArray();
            MatchInfo firstMatch = matches[0];
            MatchInfo secondMatch = matches[1];

            Assert.AreEqual(2, matches.Length);
            Assert.AreEqual(1, firstMatch.LineNumber);
            Assert.AreEqual(2, secondMatch.LineNumber);
            Assert.AreEqual("first line", firstMatch.Line);
            Assert.AreEqual("second line", secondMatch.Line);
            Assert.AreEqual("first", firstMatch.Pattern);
            Assert.AreEqual("line", secondMatch.Pattern);
            Assert.AreEqual(0, firstMatch.Matches.Single().Index);
            Assert.AreEqual(5, firstMatch.Matches.Single().Length);
            Assert.AreEqual(7, secondMatch.Matches.Single().Index);
            Assert.AreEqual(4, secondMatch.Matches.Single().Length);
        }

        [Test]
        public void TwoPatternsPassedSecondPattermMatchesFirstLine()
        {
            string fileName = CreateFile("first line", ".txt");

            string command = string.Format("Select-String -Path '{0}' -Pattern 'nomatch','line'", fileName);
            MatchInfo match = ReferenceHost.RawExecute(command)
                 .Select(psObject => (MatchInfo)psObject.ImmediateBaseObject)
                 .Single();
            Match regexMatch = match.Matches.Single();

            Assert.AreEqual(1, match.LineNumber);
            Assert.AreEqual("first line", match.Line);
            Assert.AreEqual("line", match.Pattern);
            Assert.IsTrue(regexMatch.Success);
            Assert.AreEqual(6, regexMatch.Index);
            Assert.AreEqual(4, regexMatch.Length);
        }

        [Test]
        public void TwoFilesPassedMatchInBothFiles()
        {
            string fileName1 = CreateFile("first line", ".txt");
            string fileName2 = CreateFile("first line", ".txt");

            string command = string.Format("Select-String -Path '{0}','{1}' -Pattern 'first'", fileName1, fileName2);
            MatchInfo[] matches = ReferenceHost.RawExecute(command)
                .Select(psObject => (MatchInfo)psObject.ImmediateBaseObject)
                .ToArray();
            MatchInfo firstMatch = matches[0];
            MatchInfo secondMatch = matches[1];

            Assert.AreEqual(fileName1, firstMatch.Path);
            Assert.AreEqual(fileName2, secondMatch.Path);
        }
    }
}
