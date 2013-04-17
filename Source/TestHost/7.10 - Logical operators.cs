using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHost
{
    [TestFixture]
    class LogicalOperators
    {
        [Test, Explicit]
        [TestCase(@"($j -gt 5) -and (++$k -lt 15)		# True -and False ->", "False")]
        [TestCase(@"($j -gt 5) -and ($k -le 21)		# True -and True ->", "True")]
        [TestCase(@"($j++ -gt 5) -and ($j -le 10)		# True -and False ->", "False")]
        [TestCase(@"($j -eq 5) -and (++$k -gt 15)		# False -and True ->", "False")]
        public void And(string input, string expected)
        {
            var result = TestHost.Execute(
                @"$j = 10",
                @"$k = 20",
                input
                );

            Assert.AreEqual(expected + Environment.NewLine, result);
        }

        [Test]
        [TestCase(@"($j++ -gt 5) -or (++$k -lt 15)	# True -or False ->", "True", Explicit = true)]
        [TestCase(@"($j -eq 10) -or ($k -gt 15)		# False -or True ->", "True")]
        [TestCase(@"($j -eq 10) -or (++$k -le 20)		# False -or False ->", "False", Explicit = true)]
        public void Or(string input, string expected)
        {
            var result = TestHost.Execute(
                @"$j = 10",
                @"$k = 20",
                input
                );

            Assert.AreEqual(expected + Environment.NewLine, result);
        }

        [Test, Explicit]
        [TestCase(@"($j++ -gt 5) -xor (++$k -lt 15)	# True -xor False ->", "True")]
        [TestCase(@"($j -eq 10) -xor ($k -gt 15)		# False -xor True ->", "True")]
        [TestCase(@"($j -gt 10) -xor (++$k -le 25)	# True -xor True ->", "False")]
        public void Xor(string input, string expected)
        {
            var result = TestHost.Execute(
                @"$j = 10",
                @"$k = 20",
                input
                );

            Assert.AreEqual(expected + Environment.NewLine, result);
        }
    }
}
