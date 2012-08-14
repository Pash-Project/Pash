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
    }

    class PowerShellGrammar : Grammar
    {
    }
}

