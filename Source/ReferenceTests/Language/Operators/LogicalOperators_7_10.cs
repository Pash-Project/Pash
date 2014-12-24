using System;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Language.Operators
{
    [TestFixture]
    public class LogicalOperators_7_10 : ReferenceTestBase
    {
        [Test]
        [TestCase(@"$c = @(); $c -and $true                         # False -and True ->", "False")]
        [TestCase(@"$c = @(); $true -and $c                         # True -and False ->", "False")]
        [TestCase(@"$c = @(); $c.length -and $true                  # False -and True ->", "False")]
        [TestCase(@"$c = New-Object System.Version; $c -and $true   # True -and True ->", "True")]
        [TestCase(@"$c = New-Object System.Version; $true -and $c   # True -and True ->", "True")]
        [TestCase(@"$c = 1,2; $c.length -and $true                  # True -and True ->", "True")]
        [TestCase(@"$null -and $true                                # False -and True ->", "False")]
        [TestCase(@"$true -and $null                                # True -and False ->", "False")]
        [TestCase(@"$c = 'abc'; $c -and $true                       # True -and True ->", "True")]
        [TestCase("$c = \"\"; $c -and $true                         # False -and True ->", "False")]
        [TestCase(@"$c = 'abc'; $true -and $c                       # True -and True ->", "True")]
        [TestCase("$c = \"\"; $true -and $c                         # True -and False ->", "False")]
        [TestCase("1 -and $true                                     # True -and True ->", "True")]
        [TestCase("0 -and $true                                     # False -and True ->", "False")]
        [TestCase("$true -and 1                                     # True -and True ->", "True")]
        [TestCase("$true -and 0                                     # True -and False ->", "False")]
        [TestCase("[char]65 -and $true                              # True -and True ->", "True")]
        [TestCase("[char]0 -and $true                               # False -and True ->", "False")]
        [TestCase("$true -and [char]65                              # True -and True ->", "True")]
        [TestCase("$true -and [char]0                               # True -and False ->", "False")]
        public void AndWithNonBooleanOperands(string input, string expected)
        {
            string result = ReferenceHost.Execute(input);

            Assert.AreEqual(expected + Environment.NewLine, result);
        }

        [Test]
        [TestCase(@"$c = @(); $c -or $false                         # False -or False ->", "False")]
        [TestCase(@"$c = @(); $false -or $c                         # False -or False ->", "False")]
        [TestCase(@"$c = @(); $c.length -or $true                   # False -or True ->", "True")]
        [TestCase(@"$c = New-Object System.Version; $c -or $false   # True -or False ->", "True")]
        [TestCase(@"$c = New-Object System.Version; $false -or $c   # False -or True ->", "True")]
        [TestCase(@"$c = 1,2; $c.length -or $false                  # True -or False ->", "True")]
        [TestCase(@"$null -or $false                                # False -or False ->", "False")]
        [TestCase(@"$false -or $null                                # False -or false ->", "False")]
        [TestCase(@"$c = 'abc'; $c -or $false                       # True -or False ->", "True")]
        [TestCase("$c = \"\"; $c -or $false                         # False -or False ->", "False")]
        [TestCase(@"$c = 'abc'; $false -or $c                       # False -or True ->", "True")]
        [TestCase("$c = \"\"; $false -or $c                         # False -or False ->", "False")]
        [TestCase("1 -or $false                                     # True -or False ->", "True")]
        [TestCase("0 -or $false                                     # False -or False ->", "False")]
        [TestCase("$false -or 1                                     # False -or True ->", "True")]
        [TestCase("$false -or 0                                     # False -or False ->", "False")]
        [TestCase("[char]65 -or $false                              # True -or False ->", "True")]
        [TestCase("[char]0 -or $false                               # False -or False ->", "False")]
        [TestCase("$false -or [char]65                              # False -or True ->", "True")]
        [TestCase("$false -or [char]0                               # False -or False ->", "False")]
        public void OrWithNonBooleanOperands(string input, string expected)
        {
            string result = ReferenceHost.Execute(input);

            Assert.AreEqual(expected + Environment.NewLine, result);
        }

        [Test]
        [TestCase(@"$c = @(); $c -xor $true                          # False -xor True ->", "True")]
        [TestCase(@"$c = @(); $false -xor $c                         # False -xor False ->", "False")]
        [TestCase(@"$c = @(); $c.length -xor $true                   # False -xor True ->", "True")]
        [TestCase(@"$c = New-Object System.Version; $c -xor $true    # True -xor True ->", "False")]
        [TestCase(@"$c = New-Object System.Version; $true -xor $c    # True -xor True ->", "False")]
        [TestCase(@"$c = 1,2; $c.length -xor $true                   # True -xor True ->", "False")]
        [TestCase(@"$null -xor $true                                 # False -xor True ->", "True")]
        [TestCase(@"$false -xor $null                                # False -xor False ->", "False")]
        [TestCase(@"$c = 'abc'; $c -xor $true                        # True -xor True ->", "False")]
        [TestCase("$c = \"\"; $c -xor $true                          # False -xor True ->", "True")]
        [TestCase(@"$c = 'abc'; $true -xor $c                        # True -xor True ->", "False")]
        [TestCase("$c = \"\"; $false -xor $c                         # False -xor False ->", "False")]
        [TestCase("1 -xor $true                                      # True -xor True ->", "False")]
        [TestCase("0 -xor $true                                      # False -xor True ->", "True")]
        [TestCase("$true -xor 1                                      # True -xor True ->", "False")]
        [TestCase("$false -xor 0                                     # False -xor False ->", "False")]
        [TestCase("[char]65 -xor $true                               # True -xor True ->", "False")]
        [TestCase("[char]0 -xor $true                                # False -xor True ->", "True")]
        [TestCase("$true -xor [char]65                               # True -xor True ->", "False")]
        [TestCase("$false -xor [char]0                                # False -xor False ->", "False")]
        public void XorWithNonBooleanOperands(string input, string expected)
        {
            string result = ReferenceHost.Execute(input);

            Assert.AreEqual(expected + Environment.NewLine, result);
        }
    }
}
