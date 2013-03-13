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
        public void IfTest()
        {
            var parseTree = PowerShellGrammar.Parser.Parse(@"if ($true) {}");

            if (parseTree.HasErrors())
            {
                Assert.Fail(parseTree.ParserMessages[0].ToString());
            }
        }

        [Test]
        public void IfElseTest()
        {
            var parseTree = PowerShellGrammar.Parser.Parse(@"if ($true) {} else {}");

            if (parseTree.HasErrors())
            {
                Assert.Fail(parseTree.ParserMessages[0].ToString());
            }
        }

        [Test]
        public void IfElseIfTest()
        {
            var parseTree = PowerShellGrammar.Parser.Parse(@"if ($true) {} elseif ($true) {} ");

            if (parseTree.HasErrors())
            {
                Assert.Fail(parseTree.ParserMessages[0].ToString());
            }
        }

        [Test, Explicit("bug")]
        public void IfElseIfElseTest()
        {
            var parseTree = PowerShellGrammar.Parser.Parse(@"if ($true) {} elseif {$true) {} else {}");

            if (parseTree.HasErrors())
            {
                Assert.Fail(parseTree.ParserMessages[0].ToString());
            }
        }


        [Test, Explicit("bug")]
        public void IfElseifElseTest()
        {
            var parseTree = PowerShellGrammar.Parser.Parse(@"if ($true) {} elseif ($true) {} elseif ($true) else {}");

            if (parseTree.HasErrors())
            {
                Assert.Fail(parseTree.ParserMessages[0].ToString());
            }
        }
    }
}
