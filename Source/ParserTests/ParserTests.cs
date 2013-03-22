using NUnit.Framework;
using Pash.ParserIntrinsics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserTests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        [TestCase(@"if ($true) {} else {}")]
        [TestCase(@"if ($true) {} elseif ($true) {} ")]
        [TestCase(@"if ($true) {} elseif {$true) {} else {}", Explicit = true)]
        [TestCase(@"if ($true) {} elseif ($true) {} elseif ($true) else {}", Explicit = true)]
        public void IfElseSyntax(string input)
        {
            var parseTree = PowerShellGrammar.Parser.Parse(input);

            if (parseTree.HasErrors())
            {
                Assert.Fail(parseTree.ParserMessages[0].ToString());
            }
        }

        [Test]
        [TestCase(@"[int]")]
        [TestCase(@"[int]-3"), Description("Cast, not subtraction")]
        [TestCase(@"[int],[string]")]
        [TestCase(@"$x = [int]")]
        [TestCase(@"$x::MaxValue")]
        [TestCase(@"[int]::Parse()", Explicit = true)]
        [TestCase(@"$x::Parse()", Explicit = true)]
        [TestCase(@"$x.Assembly")]
        [TestCase(@"$x.AsType()", Explicit = true)]
        public void TypesAndMembers(string input)
        {
            var parseTree = PowerShellGrammar.Parser.Parse(input);

            if (parseTree.HasErrors())
            {
                Assert.Fail(parseTree.ParserMessages[0].ToString());
            }
        }
    }
}
