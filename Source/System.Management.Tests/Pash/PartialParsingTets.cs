using System;
using NUnit.Framework;
using Pash.Implementation;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace ParserTests
{
    [TestFixture]
    public class PartialParsingTets
    {
        [TestCase("'")]
        [TestCase("$")]
        public void PartialParsingAlsoThrowsOnInvalidSyntax(string input)
        {
            ScriptBlockAst sb;
            Assert.Throws<ParseException>(delegate {
                Parser.TryParsePartialInput(input + Environment.NewLine, out sb);
            });
        }

        [TestCase("if ('foo') { ")]
        [TestCase("if ($true) { 'foo' } else {")]
        [TestCase("@(")]
        [TestCase("@{")]
        [TestCase("foreach ($a in $b)")]
        [TestCase("foo | foreach {")]
        [TestCase("'foo' | ")]
        [TestCase("$object.method(")]
        [TestCase("function f {")]
        public void PartialParsingIsPossible(string input)
        {
            ScriptBlockAst sb;
            var complete = Parser.TryParsePartialInput(input + Environment.NewLine, out sb);
            Assert.That(complete, Is.False, "Parser reported complete parsing of incomplete sentence");
        }

        [TestCase("1")]
        [TestCase("$var")]
        [TestCase("if ($true) { 'foo' }")]
        [TestCase("'foo' | bar")]
        [TestCase("function f { 'test' }")]
        public void PartialParsingRecognizesCompleteInput(string input)
        {
            ScriptBlockAst sb;
            var complete = Parser.TryParsePartialInput(input, out sb);
            Assert.That(complete, Is.True, "Parser reported incomplete parsing of complete sentence");
        }
    }
}

