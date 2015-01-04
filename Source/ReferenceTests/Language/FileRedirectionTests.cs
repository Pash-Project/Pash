using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class FileRedirectionTests : ReferenceTestBase
    {
        private string _tempFileName;

        private string GenerateTempFileName(string fileName = "outputfile.txt")
        {
            _tempFileName = Path.Combine(Path.GetTempPath(), fileName);
            AddCleanupFile(_tempFileName);
            return _tempFileName;
        }

        private void AssertTempFileContains(params string[] lines)
        {
            AssertFileContains(_tempFileName, lines);
        }

        private void AssertFileContains(string fileName, params string[] lines)
        {
            Assert.AreEqual(NewlineJoin(lines), NewlineJoin(ReadLinesFromFile(fileName)));
        }

        [Test]
        public void OutputStreamToFile()
        {
            string fileName = GenerateTempFileName();
            ReferenceHost.Execute(new string[] {
                "$i = 10",
                "$i > " + fileName});

            AssertTempFileContains("10");
        }

        [Test]
        public void OutputStreamToFileNameInSingleQuotes()
        {
            string fileName = GenerateTempFileName();
            ReferenceHost.Execute("'foobar' > '" + fileName + "'");

            AssertTempFileContains("foobar");
        }

        [Test]
        public void CommandOutputToFile()
        {
            string fileName = GenerateTempFileName();
            ReferenceHost.Execute("get-command | ? { $_.name -eq 'where-object' } | % { $_.name } > " + fileName);

            AssertTempFileContains("Where-Object");
        }

        [Test]
        public void OutputStreamToSameFileTwiceShouldOverwriteExistingFile()
        {
            string fileName = GenerateTempFileName();
            ReferenceHost.Execute(new string[] {
                "'abc' > " + fileName,
                "'def' > " + fileName});

            AssertTempFileContains("def");
        }

        [Test]
        public void AppendOutputStreamToFileThatDoesNotExist()
        {
            string fileName = GenerateTempFileName();
            ReferenceHost.Execute("'abc' >> " + fileName);

            AssertTempFileContains("abc");
        }

        [Test]
        public void AppendOutputStreamToSameFileShouldNotOverwriteExistingFile()
        {
            string fileName = GenerateTempFileName();
            ReferenceHost.Execute(new string[] {
                "'abc' > " + fileName,
                "'def' >> " + fileName});

            AssertTempFileContains("abc", "def");
        }
    }
}
