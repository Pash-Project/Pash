using System;
using System.Management.Automation.Language;
using NUnit.Framework;

namespace ParserTests
{
    public class ExpectedScriptExtent : IScriptExtent
    {
        public int EndColumnNumber { get; set; }
        public int EndLineNumber { get; set; }
        public int EndOffset { get; set; }
        public string File { get; set; }
        public int StartColumnNumber { get; set; }
        public int StartLineNumber { get; set; }
        public int StartOffset { get; set; }
        public string Text { get; set; }

        public void AssertAreEqual(IScriptExtent extent)
        {
            string expected = ToString(this);
            string actual = ToString(extent);
            Assert.AreEqual(expected, actual);
        }

        private string ToString(IScriptExtent extent)
        {
            return string.Format("Text='{0}' Offsets=[{1},{2}] Columns=[{3},{4}] Lines=[{5},{6}]",
                extent.Text,
                extent.StartOffset,
                extent.EndOffset,
                extent.StartColumnNumber,
                extent.EndColumnNumber,
                extent.StartLineNumber,
                extent.EndLineNumber);
        }

        public IScriptPosition EndScriptPosition { get; set; }
        public IScriptPosition StartScriptPosition { get; set; }
    }
}
