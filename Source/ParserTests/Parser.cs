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
            Assert.IsNotNull(grammar);
        }

        [Test]
        public void CaseInsenstiveParserTest()
        {
            var grammar = new PowerShellGrammar();

            Assert.IsTrue(grammar.CaseSensitive);
        }
    }

    abstract class CaseInsensitiveGrammar : Grammar
    {
        public CaseInsensitiveGrammar()
            : base(true)
        {
        }
    }

    class PowerShellGrammar : CaseInsensitiveGrammar
    {
        public PowerShellGrammar()
        {
        }
    }
}

