using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Irony.Parsing;
using StringExtensions;

namespace ParserTests
{
    [TestFixture]
    class PowerShellGrammarTests
    {
        [Test]
        public void CreateTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();
            // obviously I know it won't be null. That's mostly to 
            // avoid the compiler warning.
            Assert.IsNotNull(grammar);
        }

        [Test]
        public void NonTerminalFieldsInitializedTest()
        {
            // created by reflection (to avoid missing one)
            // but let's make sure the reflection works
            var grammar = new PowerShellGrammar.InteractiveInput();
            Assert.IsNotNull(grammar.interactive_input);
            Assert.AreEqual("interactive_input", grammar.interactive_input.Name);
        }

        [Test]
        public void ParseCommandTest()
        {
            var grammar = new PowerShellGrammar.InteractiveInput();
            var parser = new Parser(grammar);

            var parseTree = parser.Parse("Get-ChildItem");

            Assert.IsNotNull(parseTree);
            Assert.IsFalse(parseTree.HasErrors, parseTree.ParserMessages.JoinString("\n"));
        }
    }
}
