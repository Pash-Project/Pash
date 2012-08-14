using System;
using NUnit.Framework;
using Irony.Parsing;

namespace ParserTests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void CreateParserTest()
        {
            var grammar = new PowerShellGrammar();
        }

        [Test]
        public void CaseInsenstiveParserTest()
        {
            var grammar = new PowerShellGrammar();

            Assert.IsTrue(grammar.CaseSensitive);
        }
    }

    class PowerShellGrammar : Grammar
    {
        public PowerShellGrammar()
            : base(true)
        {
        }
    }
}

