using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ParserTests
{
    [TestFixture]
    class PowerShellGrammarTests
    {
        [Test]
        public void CreateTest()
        {
            var grammar = new PowerShellGrammar();
            // obviously I know it won't be null. That's mostly to 
            // avoid the compiler warning.
            Assert.IsNotNull(grammar);
        }

        [Test]
        public void NonTerminalFieldsInitializedTest()
        {
            // created by reflection (to avoid missing one)
            // but let's make sure the reflection works
            var grammar = new PowerShellGrammar();
            Assert.IsNotNull(grammar.interactive_input);
            Assert.AreEqual("interactive_input", grammar.interactive_input.Name);
        }
    }
}
