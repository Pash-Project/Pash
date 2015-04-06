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
        string _originalDirectory;

        [SetUp]
        public override void SetUp()
        {
            _originalDirectory = Directory.GetCurrentDirectory();
            base.SetUp();
        }

        [TearDown]
        public override void TearDown()
        {
            Directory.SetCurrentDirectory(_originalDirectory);
            base.TearDown();
        }

        MatchInfo RawExecuteSingleMatch(string command)
        {
            return ReferenceHost.RawExecute(command)
                .Select(psObject => (MatchInfo)psObject.ImmediateBaseObject)
                .Single();
        }

        private MatchInfo[] RawExecuteMultipleMatches(string command)
        {
            return RawExecuteMultipleMatches(new[] { command });
        }

        private MatchInfo[] RawExecuteMultipleMatches(string[] commands)
        {
            return ReferenceHost.RawExecute(commands)
                .Select(psObject => (MatchInfo)psObject.ImmediateBaseObject)
                .ToArray();
        }

        private string CreateTextFile(string content, string fileName)
        {
            string temp = Path.GetTempPath();
            string directory = Path.Combine(temp, "SelectStringTests");
            Directory.CreateDirectory(directory);
            string filePath = Path.Combine(directory, fileName);
            File.WriteAllText(filePath, content);
            AddCleanupFile(filePath);
            return filePath;
        }

        [Test]
        public void FileWithSingleLineAndPatternMatchesTextInLine()
        {
            string fileName = CreateFile("first line", ".txt");

            string command = string.Format("Select-String -Path '{0}' -Pattern 'first'", fileName);
            MatchInfo match = RawExecuteSingleMatch(command);
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
        public void MatchWhenPathFirstParameterAndPatternSecondAsNamedParameter()
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
            MatchInfo[] matches = RawExecuteMultipleMatches(command);
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
            MatchInfo[] matches = RawExecuteMultipleMatches(command);
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
        public void TwoPatternsPassedSecondPatternMatchesFirstLine()
        {
            string fileName = CreateFile("first line", ".txt");

            string command = string.Format("Select-String -Path '{0}' -Pattern 'nomatch','line'", fileName);
            MatchInfo match = RawExecuteSingleMatch(command);
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
            MatchInfo[] matches = RawExecuteMultipleMatches(command);
            MatchInfo firstMatch = matches[0];
            MatchInfo secondMatch = matches[1];

            Assert.AreEqual(fileName1, firstMatch.Path);
            Assert.AreEqual(fileName2, secondMatch.Path);
        }

        [Test]
        public void MatchUsingStringFromPipeline()
        {
            MatchInfo match = RawExecuteSingleMatch("\"-HELLO-\" | select-string -pattern \"HELLO\"");
            Match regexMatch = match.Matches.Single();

            Assert.AreEqual("InputStream", match.Filename);
            Assert.AreEqual("InputStream", match.Path);
            Assert.AreEqual("-HELLO-", match.Line);
            Assert.AreEqual(1, match.LineNumber);
            Assert.AreEqual("HELLO", match.Pattern);
            Assert.AreEqual(1, regexMatch.Index);
            Assert.AreEqual(5, regexMatch.Length);
            Assert.AreEqual("HELLO", regexMatch.Value);
            Assert.AreEqual("-HELLO-", match.ToString());
        }

        [Test]
        public void MatchUsingStringArrayFromPipeline()
        {
            MatchInfo match = RawExecuteSingleMatch("\"-12345-\",\"-HELLO-\" | select-string -pattern \"HELLO\"");
            Match regexMatch = match.Matches.Single();

            Assert.AreEqual("InputStream", match.Filename);
            Assert.AreEqual("InputStream", match.Path);
            Assert.AreEqual("-HELLO-", match.Line);
            Assert.AreEqual(2, match.LineNumber);
            Assert.AreEqual("HELLO", match.Pattern);
            Assert.AreEqual(1, regexMatch.Index);
            Assert.AreEqual(5, regexMatch.Length);
            Assert.AreEqual("HELLO", regexMatch.Value);
            Assert.AreEqual("-HELLO-", match.ToString());
        }

        /// <summary>
        /// When passing an array of strings using InputObject the array is turned into a single string
        /// with a space between each item of the array.
        /// https://technet.microsoft.com/en-us/library/hh849903.aspx
        /// </summary>
        [Test]
        public void MatchUsingInputObjectStringArray()
        {
            string command = "select-string -inputobject \"-12345-\",\"-HELLO-\" -pattern \"HELLO\"";
            MatchInfo match = RawExecuteSingleMatch(command);
            Match regexMatch = match.Matches.Single();

            Assert.AreEqual("InputStream", match.Filename);
            Assert.AreEqual("InputStream", match.Path);
            Assert.AreEqual("-12345- -HELLO-", match.Line);
            Assert.AreEqual(1, match.LineNumber);
            Assert.AreEqual("HELLO", match.Pattern);
            Assert.AreEqual(9, regexMatch.Index);
            Assert.AreEqual(5, regexMatch.Length);
            Assert.AreEqual("HELLO", regexMatch.Value);
            Assert.AreEqual("-12345- -HELLO-", match.ToString());
        }

        [Test]
        public void NoMatchUsingInputObjectStringArray()
        {
            string command = "select-string -inputobject \"-12345-\",\"-HELLO-\" -pattern \"5HELLO\"";
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void MultipleMatchesUsingStringArrayFromPipeline()
        {
            MatchInfo[] matches = RawExecuteMultipleMatches("\"-12345-\",\"-HELLO-\",\"hello\" | select-string -pattern \"HELLO\"");
            MatchInfo firstMatch = matches[0];
            MatchInfo secondMatch = matches[1];

            Assert.AreEqual("InputStream", firstMatch.Filename);
            Assert.AreEqual("InputStream", secondMatch.Filename);
            Assert.AreEqual("InputStream", firstMatch.Path);
            Assert.AreEqual("InputStream", secondMatch.Path);
            Assert.AreEqual("-HELLO-", firstMatch.Line);
            Assert.AreEqual("hello", secondMatch.Line);
            Assert.AreEqual(2, firstMatch.LineNumber);
            Assert.AreEqual(3, secondMatch.LineNumber);
            Assert.AreEqual("HELLO", firstMatch.Pattern);
            Assert.AreEqual("HELLO", secondMatch.Pattern);
            Assert.AreEqual(1, firstMatch.Matches.Single().Index);
            Assert.AreEqual(5, firstMatch.Matches.Single().Length);
            Assert.AreEqual(0, secondMatch.Matches.Single().Index);
            Assert.AreEqual(5, secondMatch.Matches.Single().Length);
            Assert.AreEqual("HELLO", firstMatch.Matches.Single().Value);
            Assert.AreEqual("hello", secondMatch.Matches.Single().Value);
            Assert.AreEqual("-HELLO-", firstMatch.ToString());
            Assert.AreEqual("hello", secondMatch.ToString());
        }

        [Test]
        public void CaseSensitiveMatch()
        {
            MatchInfo match = RawExecuteSingleMatch("\"Hello\",\"HELLO\" | select-string -pattern \"HELLO\" -casesensitive");
            Match regexMatch = match.Matches.Single();

            Assert.AreEqual("InputStream", match.Filename);
            Assert.AreEqual("InputStream", match.Path);
            Assert.AreEqual("HELLO", match.Line);
            Assert.AreEqual(2, match.LineNumber);
            Assert.IsFalse(match.IgnoreCase);
            Assert.AreEqual("HELLO", match.Pattern);
            Assert.AreEqual(0, regexMatch.Index);
            Assert.AreEqual(5, regexMatch.Length);
            Assert.AreEqual("HELLO", regexMatch.Value);
        }

        [Test]
        public void RegularExpressionPattern()
        {
            MatchInfo[] matches = RawExecuteMultipleMatches("\"app\",\"app..\",\"apple\" | select-string -pattern \"app..\"");

            Assert.AreEqual(2, matches.Length);
            Assert.AreEqual("app..", matches[0].Line);
            Assert.AreEqual("apple", matches[1].Line);
        }

        [Test]
        public void SimpleMatch()
        {
            MatchInfo match = RawExecuteSingleMatch("\"app\",\"app..\",\"apple\" | select-string -SimpleMatch \"app..\"");

            Assert.AreEqual("InputStream", match.Filename);
            Assert.AreEqual("InputStream", match.Path);
            Assert.AreEqual("app..", match.Line);
            Assert.AreEqual(2, match.LineNumber);
            Assert.IsTrue(match.IgnoreCase);
            Assert.AreEqual("app..", match.Pattern);
            Assert.AreEqual(0, match.Matches.Length);
            Assert.AreEqual("app..", match.ToString());
        }

        [Test]
        public void SimpleMatchInDifferentPartsOfText()
        {
            MatchInfo[] matches = RawExecuteMultipleMatches("\"app.a\",\"aapp..\",\"dapp.\",\"appp\" | select-string -SimpleMatch \"app.\"");

            Assert.AreEqual(3, matches.Length);
        }

        [Test]
        public void SimpleMatchIsCaseInsensitiveByDefault()
        {
            MatchInfo[] matches = RawExecuteMultipleMatches("\"App.\",\"aPp.\",\"APP.\",\"appp\" | select-string -SimpleMatch \"app.\"");

            Assert.AreEqual(3, matches.Length);
        }

        [Test]
        public void CaseSensitiveSimpleMatch()
        {
            MatchInfo[] matches = RawExecuteMultipleMatches("\"App.a\",\"app.\",\"dApp.\",\"appp\" | select-string -CaseSensitive -SimpleMatch \"app.\"");

            Assert.AreEqual(1, matches.Length);
            Assert.AreEqual("app.", matches[0].Line);
            Assert.IsFalse(matches[0].IgnoreCase);
        }

        /// <summary>
        /// Using -Quiet with values from the pipeline and -InputObject does not output $false
        /// when there is no match. This is not the case when using a file with Select-String.
        /// </summary>
        [Test]
        [TestCase("\"a\",\"b\" | select-string -quiet -pattern \"a\"", "True")]
        [TestCase("\"a\",\"b\" | select-string -quiet -pattern \"c\"", "")]
        [TestCase("\"a\",\"b\" | select-string -quiet -pattern \"A\"", "True")]
        [TestCase("\"a\",\"b\" | select-string -quiet -pattern \"A\" -casesensitive", "")]
        [TestCase("\"A\",\"a\" | select-string -quiet -pattern \"A\" -casesensitive -notmatch", "True")]
        [TestCase("\"a\",\"aa\" | select-string -quiet -pattern \"a\"", "True")]
        [TestCase("\"a\",\"a\" | select-string -quiet -pattern \"a\" -notmatch", "")]
        [TestCase("\"aa\",\"ba\" | select-string -quiet -pattern \"b.\"", "True")]
        [TestCase("\"aa\",\"ba\" | select-string -quiet -simplematch \"b.\"", "")]
        [TestCase("\"b.\",\"ba\" | select-string -quiet -simplematch \"b.\"", "True")]
        [TestCase("\"aa\",\"b.\" | select-string -quiet -simplematch \"b.\"", "True")]
        [TestCase("\"aa\",\"b.\" | select-string -quiet -simplematch \"B.\"", "True")]
        [TestCase("\"aa\",\"b.\" | select-string -quiet -simplematch \"B.\" -casesensitive", "")]
        [TestCase("\"b\",\"b\" | select-string -quiet -simplematch \"b\"", "True")]
        [TestCase("\"b\",\"b\" | select-string -quiet -simplematch \"b\" -notmatch", "")]
        [TestCase("select-string -quiet -inputobject \"a\",\"b\" -pattern \"a\"", "True")]
        [TestCase("select-string -quiet -inputobject \"a\",\"b\" -pattern \"c\"", "")]
        [TestCase("select-string -quiet -inputobject \"a\",\"b\" -pattern \"A\"", "True")]
        [TestCase("select-string -quiet -inputobject \"a\",\"b\" -pattern \"A\" -casesensitive", "")]
        [TestCase("select-string -quiet -inputobject \"aa\",\"ba\" -pattern \"b.\"", "True")]
        [TestCase("select-string -quiet -inputobject \"aa\",\"ba\" -simplematch \"b.\"", "")]
        [TestCase("select-string -quiet -inputobject \"aa\",\"ba\" -simplematch \"b.\" -notmatch", "True")]
        [TestCase("select-string -quiet -inputobject \"aa\",\"b.\" -simplematch \"b.\"", "True")]
        [TestCase("select-string -quiet -inputobject \"aa\",\"b.\" -simplematch \"B.\"", "True")]
        [TestCase("select-string -quiet -inputobject \"aa\",\"b.\" -simplematch \"B.\" -casesensitive", "")]
        [TestCase("select-string -quiet -inputobject \"a\",\"a\" -simplematch \"a\"", "True")]
        [TestCase("select-string -quiet -inputobject \"a\",\"a\" -notmatch \"a\"", "")]
        public void QuietMatches(string command, string expectedResult)
        {
            string result = ReferenceHost.Execute(command);

            Assert.AreEqual(expectedResult, result.TrimEnd());
        }

        [Test]
        [TestCase("a,b", "select-string -quiet -pattern \"a\"", "True")]
        [TestCase("a,b", "select-string -quiet -pattern \"c\"", "False")]
        [ TestCase("a,b", "select-string -quiet -pattern \"c\" -notmatch", "True")]
        [TestCase("a,b", "select-string -quiet -pattern \"A\"", "True")]
        [TestCase("a,b", "select-string -quiet -pattern \"A\" -casesensitive", "False")]
        [TestCase("A,a", "select-string -quiet -pattern \"A\" -casesensitive -notmatch", "True")]
        [TestCase("a,a", "select-string -quiet -pattern \"A\"", "True")]
        [TestCase("A,A", "select-string -quiet -pattern \"A\" -notmatch", "False")]
        [TestCase("aa,ba", "select-string -quiet -pattern \"b.\"", "True")]
        [TestCase("aa,ba", "select-string -quiet -simplematch \"b.\"", "False")]
        [TestCase("aa,ba", "select-string -quiet -simplematch \"b.\" -notmatch", "True")]
        [TestCase("aa,b.", "select-string -quiet -simplematch \"b.\"", "True")]
        [TestCase("b.,b.", "select-string -quiet -simplematch \"b.\" -notmatch", "False")]
        [TestCase("aa,b.", "select-string -quiet -simplematch \"B.\"", "True")]
        [TestCase("aa,b.", "select-string -quiet -simplematch \"B.\" -casesensitive", "False")]
        [TestCase("b.,b.", "select-string -quiet -simplematch \"B.\" -casesensitive -notmatch", "True")]
        [TestCase("b,b", "select-string -quiet -simplematch \"B\"", "True")]
        public void QuietMatchesInFile(string fileContent, string command, string expectedResult)
        {
            string[] lines = fileContent.Split(',');
            string fileName = CreateFile(NewlineJoin(lines), ".txt");
            string fullCommand = string.Format("{0} -Path '{1}'", command, fileName);
            string result = ReferenceHost.Execute(fullCommand);

            Assert.AreEqual(expectedResult + Environment.NewLine, result);
        }

        [Test]
        public void FileWildcardMatch()
        {
            string fileName = CreateTextFile("a", "foo1.txt");
            string directory = Path.GetDirectoryName(fileName);
            AddCleanupDir(directory);
            CreateTextFile("ba", "foo2.txt");
            CreateTextFile("aa", "bar1.txt");
            CreateTextFile("baa", "bar2.txt");

            MatchInfo[] matches = RawExecuteMultipleMatches(new [] {
                string.Format("cd '{0}'", directory),
                "select-string -Pattern a -Path foo?.txt,bar?.txt"
            });

            Assert.AreEqual(4, matches.Length);
            Assert.AreEqual("foo1.txt", matches[0].Filename);
            Assert.AreEqual("foo2.txt", matches[1].Filename);
            Assert.AreEqual("bar1.txt", matches[2].Filename);
            Assert.AreEqual("bar2.txt", matches[3].Filename);
            Assert.AreEqual("a", matches[0].Line);
            Assert.AreEqual("ba", matches[1].Line);
            Assert.AreEqual("aa", matches[2].Line);
            Assert.AreEqual("baa", matches[3].Line);
        }

        [Test]
        public void FileWildcardWithInclude()
        {
            string fileName = CreateTextFile("a", "foo1.txt");
            string directory = Path.GetDirectoryName(fileName);
            AddCleanupDir(directory);
            CreateTextFile("ba", "foo2.txt");
            CreateTextFile("aa", "bar1.txt");
            CreateTextFile("baa", "bar2.txt");

            MatchInfo[] matches = RawExecuteMultipleMatches(new[] {
                string.Format("cd '{0}'", directory),
                "select-string -Pattern a -Path *.txt -include *foo1.txt"
            });

            Assert.AreEqual(1, matches.Length);
            Assert.AreEqual("foo1.txt", matches[0].Filename);
            Assert.AreEqual("a", matches[0].Line);
        }

        [Test]
        public void FileWildcardWithExclude()
        {
            string fileName = CreateTextFile("a", "foo1.txt");
            string directory = Path.GetDirectoryName(fileName);
            AddCleanupDir(directory);
            CreateTextFile("ba", "foo2.txt");
            CreateTextFile("aa", "bar1.txt");
            CreateTextFile("baa", "bar2.txt");

            MatchInfo[] matches = RawExecuteMultipleMatches(new[] {
                string.Format("cd '{0}'", directory),
                "select-string -Pattern a -Path *.txt -exclude *foo?.txt"
            });

            Assert.AreEqual(2, matches.Length);
            Assert.AreEqual("bar1.txt", matches[0].Filename);
            Assert.AreEqual("aa", matches[0].Line);
            Assert.AreEqual("bar2.txt", matches[1].Filename);
            Assert.AreEqual("baa", matches[1].Line);
        }

        [Test]
        public void LiteralPaths()
        {
            string fileName = CreateTextFile("a", "File[1].txt");
            string directory = Path.GetDirectoryName(fileName);
            CreateTextFile("ba", "File[2].txt");
            AddCleanupDir(directory);

            MatchInfo[] matches = RawExecuteMultipleMatches(new[] {
                string.Format("cd '{0}'", directory),
                "select-string -Pattern a -LiteralPath File[1].txt,File[2].txt"
            });

            Assert.AreEqual(2, matches.Length);
            Assert.AreEqual("File[1].txt", matches[0].Filename);
            Assert.AreEqual("File[2].txt", matches[1].Filename);
            Assert.AreEqual("a", matches[0].Line);
            Assert.AreEqual("ba", matches[1].Line);
        }

        [Test]
        [TestCase("a,b", "select-string -quiet -pattern \"a\"", "True")]
        [TestCase("b,b", "select-string -quiet -simplematch \"B\"", "True")]
        public void QuietMatchesInLiteralFilePath(string fileContent, string command, string expectedResult)
        {
            string[] lines = fileContent.Split(',');
            string fileName = CreateFile(NewlineJoin(lines), ".txt");
            string fullCommand = string.Format("{0} -LiteralPath '{1}'", command, fileName);
            string result = ReferenceHost.Execute(fullCommand);

            Assert.AreEqual(expectedResult + Environment.NewLine, result);
        }

        [Test]
        [TestCase("a,a", "select-string -List -SimpleMatch \"a\"")]
        [TestCase("a,a", "select-string -List -Pattern \"a\"")]
        [TestCase("a,a", "select-string -List -SimpleMatch \"b\" -NotMatch")]
        [TestCase("a,a", "select-string -List -Pattern \"b\" -NotMatch")]
        public void ListArgumentMatchesOnlyOneItem(string fileContent, string command)
        {
            string[] lines = fileContent.Split(',');
            string fileName = CreateFile(NewlineJoin(lines), ".txt");
            string fullCommand = string.Format("{0} -Path '{1}'", command, fileName);
            MatchInfo[] matches = RawExecuteMultipleMatches(fullCommand);

            Assert.AreEqual(1, matches.Length);
            Assert.AreEqual("a", matches[0].Line);
        }

        [Test]
        public void ListArgumentMatchesOnlyOneItemInEachFile()
        {
            string fileName = CreateTextFile(NewlineJoin("a","ab"), "foo1.txt");
            string directory = Path.GetDirectoryName(fileName);
            AddCleanupDir(directory);
            CreateTextFile(NewlineJoin("aa", "ab"), "foo2.txt");

            MatchInfo[] matches = RawExecuteMultipleMatches(new[] {
                string.Format("cd '{0}'", directory),
                "select-string -Pattern a -Path foo?.txt -List"
            });

            Assert.AreEqual(2, matches.Length);
            Assert.AreEqual("a", matches[0].Line);
            Assert.AreEqual("foo1.txt", matches[0].Filename);
            Assert.AreEqual("aa", matches[1].Line);
            Assert.AreEqual("foo2.txt", matches[1].Filename);
        }

        [Test]
        [TestCase("'a','aa' | Select-String -Pattern a -List")]
        [TestCase("'a','aa' | Select-String -Pattern b -List -NotMatch")]
        public void ListArgumentWhenItemsPassedOnPipeline(string command)
        {
            MatchInfo[] matches = RawExecuteMultipleMatches(command);

            Assert.AreEqual(2, matches.Length);
            Assert.AreEqual("a", matches[0].Line);
            Assert.AreEqual("aa", matches[1].Line);
        }

        [Test]
        public void AllMatchesFromPipeline()
        {
            MatchInfo[] matches = RawExecuteMultipleMatches("'aa','aaa' | select-string -pattern a -AllMatches");
            MatchInfo firstMatch = matches[0];
            MatchInfo secondMatch = matches[1];

            Assert.AreEqual(2, matches.Length);
            Assert.AreEqual("aa", firstMatch.Line);
            Assert.AreEqual(2, firstMatch.Matches.Length);
            Assert.AreEqual(0, firstMatch.Matches[0].Index);
            Assert.AreEqual("a", firstMatch.Matches[0].Value);
            Assert.AreEqual(1, firstMatch.Matches[0].Length);
            Assert.AreEqual(1, firstMatch.Matches[1].Index);
            Assert.AreEqual("a", firstMatch.Matches[1].Value);
            Assert.AreEqual(1, firstMatch.Matches[1].Length);
            Assert.AreEqual("aaa", secondMatch.Line);
            Assert.AreEqual(3, secondMatch.Matches.Length);
            Assert.AreEqual(0, secondMatch.Matches[0].Index);
            Assert.AreEqual("a", secondMatch.Matches[0].Value);
            Assert.AreEqual(1, secondMatch.Matches[0].Length);
            Assert.AreEqual(1, secondMatch.Matches[1].Index);
            Assert.AreEqual("a", secondMatch.Matches[1].Value);
            Assert.AreEqual(1, secondMatch.Matches[1].Length);
            Assert.AreEqual(2, secondMatch.Matches[2].Index);
            Assert.AreEqual("a", secondMatch.Matches[2].Value);
            Assert.AreEqual(1, secondMatch.Matches[2].Length);
        }

        [Test]
        public void NotMatchFromPipeline()
        {
            MatchInfo match = RawExecuteSingleMatch("'a','b' | select-string -pattern a -NotMatch");

            Assert.AreEqual("b", match.Line);
            Assert.AreEqual("InputStream", match.Path);
            Assert.AreEqual("InputStream", match.Filename);
            Assert.AreEqual(2, match.LineNumber);
            Assert.AreEqual(0, match.Matches.Length);
            Assert.IsTrue(match.IgnoreCase);
        }

        [Test]
        public void NotMatchFromFile()
        {
            string fileName = CreateFile(NewlineJoin("a", "b"), ".txt");
            string command = string.Format("Select-String -Path '{0}' -Pattern a -NotMatch", fileName);
            MatchInfo match = RawExecuteSingleMatch(command);

            Assert.AreEqual("b", match.Line);
            Assert.AreEqual(2, match.LineNumber);
            Assert.AreEqual(0, match.Matches.Length);
            Assert.AreEqual(fileName, match.Path);
            Assert.AreEqual(Path.GetFileName(fileName), match.Filename);
        }

        [Test]
        public void CaseSensitiveNotMatchFromPipeline()
        {
            MatchInfo match = RawExecuteSingleMatch("'b','B' | select-string -pattern b -NotMatch -CaseSensitive");

            Assert.AreEqual("B", match.Line);
            Assert.AreEqual(2, match.LineNumber);
            Assert.AreEqual(0, match.Matches.Length);
            Assert.IsFalse(match.IgnoreCase);
        }

        [Test]
        public void NotMatchMultiplePatternsFromPipelineReturnsOneNotMatchOnlyEvenIfAllPatternsAreNotMatch()
        {
            MatchInfo match = RawExecuteSingleMatch("'a' | select-string -pattern b,b -NotMatch");

            Assert.AreEqual("a", match.Line);
            Assert.AreEqual("InputStream", match.Path);
            Assert.AreEqual("InputStream", match.Filename);
            Assert.AreEqual(1, match.LineNumber);
            Assert.AreEqual(0, match.Matches.Length);
            Assert.IsTrue(match.IgnoreCase);
        }
    }
}
