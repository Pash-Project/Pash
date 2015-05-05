using System;
using System.Linq;
using System.Management.Automation;
using NUnit.Framework;

namespace ReferenceTests.Language.Operators
{
    ///
    /// RelationshipOperators:
    ///     - GreaterThan
    ///     - GreaterThanEquals
    ///     - LessThan
    ///     - LessThanEquals
    ///     - Equals
    ///     - NotEquals
    ///
    /// Organize Tests based on LHS Operand Types
    /// Test Cases are reused for each Relationship Operation
    /// Lowercase < Uppercase
    ///
    /// Organized for Primitive Types:
    ///     - String
    ///     - DateTime
    ///     - Double
    ///     - Float
    ///     - Long
    ///     - Int
    ///     - Char
    ///     - Byte
    ///     - Bool
    ///
    [TestFixture]
    public class RelationshipOperators_7_9 : ReferenceTestBase
    {
        [Test]
        [TestCase("$null -gt $true                                  ", "False")]
        [TestCase("$null -igt 0                                     ", "False")]
        [TestCase("$null -cgt \"abc\"                               ", "False")]
        [TestCase("$null -ge $false                                 ", "False")]
        [TestCase("$null -ige 1.0                                   ", "False")]
        [TestCase("$null -cge [float] -1.0                          ", "True")]
        [TestCase("$null -cgt [double] -1.0                         ", "True")]
        [TestCase("$null -ilt [long] -1                             ", "False")]
        [TestCase("$null -clt -1                                    ", "False")]
        [TestCase("$d = Get-Date; $null -lt $d;                     ", "True")]
        [TestCase("$c = [char] 0x1010; $null -ilt $c;               ", "True")]
        [TestCase("$b = [byte] 0x10; $null -clt $b;                 ", "True")]
        [TestCase("$null -le $true;                                 ", "True")]
        [TestCase("$null -ile 0;                                    ", "True")]
        [TestCase("$null -cle \"abc\";                              ", "True")]
        public void RelationshipOperatorsWithNull(string input, string expected)
        {
            string result = ReferenceHost.Execute(input);
            Assert.AreEqual(expected + Environment.NewLine, result);
        }


        [Test]
            /// -gt, -igt, -cgt
        [TestCase("\"a\" -gt $null                                      ", "True")]
        [TestCase("\"u\" -gt $true                                      ", "True")]
        [TestCase("\"a\" -igt \"B\"                                     ", "False")]
        [TestCase("\"a\" -cgt \"A\"                                     ", "False")]
        [TestCase("$d = Get-Date \"01/01/2015\"; \"01/02/2015\" -gt $d; ", "True")]
        [TestCase("\"1.23450\" -gt 1.2345                               ", "True")]
        [TestCase("\"1.23450\" -gt [float] 1.2345                       ", "True")]
        [TestCase("\"2\" -gt [long] 1                                   ", "True")]
        [TestCase("\"0\" -gt 1                                          ", "False")]
        [TestCase("$c = [char] 0x1000; \"1025\" -gt $c;                 ", "False")]
        [TestCase("$b = [byte] 0x10; \"17\" -gt $b;                     ", "True")]
            /// -ge, -ige, -cge
        [TestCase("\"a\" -ge $null                                      ", "True")]
        [TestCase("\"t\" -ge $true                                      ", "False")]
        [TestCase("\"a\" -ige \"A\"                                     ", "True")]
        [TestCase("\"a\" -cge \"A\"                                     ", "False")]
        [TestCase("$d = Get-Date \"01/01/2015\"; \"01/01/2015\" -ge $d; ", "False")]
        [TestCase("\"1.2345\" -ge 1.2345                                ", "True")]
        [TestCase("\"1.2345\" -ge [float] 1.2345                        ", "True")]
        [TestCase("\"1\" -ge [long] 1                                   ", "True")]
        [TestCase("\"1\" -ge 1                                          ", "True")]
        [TestCase("$c = [char] 0x1000; \"1024\" -ge $c;                 ", "False")]
        [TestCase("$b = [byte] 0x10; \"16\" -ge $b;                     ", "True")]
            /// -lt, -ilt, -clt
        [TestCase("\"a\" -lt $null                                      ", "False")]
        [TestCase("\"t\" -lt $true                                      ", "True")]
        [TestCase("\"a\" -ilt \"A\"                                     ", "False")]
        [TestCase("\"a\" -clt \"A\"                                     ", "True")]
        [TestCase("\"[\" -clt \"{\"                                     ", "True")]
        [TestCase("$d = Get-Date \"01/01/2015\"; \"01/01/2015\" -lt $d; ", "True")]
        [TestCase("\"1.2345\" -lt 1.2345                                ", "False")]
        [TestCase("\"1.2345\" -lt [float] 1.2345                        ", "False")]
        [TestCase("\"1\" -lt [long] 1                                   ", "False")]
        [TestCase("\"1\" -lt 1                                          ", "False")]
        [TestCase("$c = [char] 0x1000; \"1024\" -lt $c;                 ", "True")]
        [TestCase("$b = [byte] 0x10; \"16\" -lt $b;                     ", "False")]
            /// -le, -ile, -cle
        [TestCase("\"a\" -le $null                                      ", "False")]
        [TestCase("\"t\" -le $true                                      ", "True")]
        [TestCase("\"a\" -ile \"A\"                                     ", "True")]
        [TestCase("\"a\" -cle \"A\"                                     ", "True")]
        [TestCase("$d = Get-Date \"01/01/2015\"; \"01/01/2015\" -le $d; ", "True")]
        [TestCase("\"1.2345\" -le 1.2345                                ", "True")]
        [TestCase("\"1.2345\" -le [float] 1.2345                        ", "True")]
        [TestCase("\"1\" -le [long] 1                                   ", "True")]
        [TestCase("\"1\" -le 1                                          ", "True")]
        [TestCase("$c = [char] 0x1000; \"1024\" -le $c;                 ", "True")]
        [TestCase("$b = [byte] 0x10; \"16\" -le $b;                     ", "True")]
        public void RelationshipOperatorsWithString(string input, string expected)
        {
            string result = ReferenceHost.Execute(input);
            Assert.AreEqual(expected + Environment.NewLine, result);
        }


        [Test]
            /// -gt, -igt, -cgt
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -gt $null               ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -gt $true               ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -igt \"Jan. 1st. 2014\" ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -cgt \"Jan. 1st. 2014\" ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -gt \"01/01/2014\"      ", "True")]
        [TestCase("(Get-Date \"01/01/2015\") -gt (Get-Date \"01/01/2015\")  ", "False")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -gt 1.2345              ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -gt [float] 1.2345      ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -gt [long] 1            ", "True")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -gt 1                   ", "True")]
        [TestCase("(Get-Date \"01/01/2015\") -gt [char] 0x1000              ", "True")]
        [TestCase("(Get-Date \"01/01/2015\") -gt [byte] 0x10                ", "True")]
            /// -ge, -ige, -cge
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -ge $null               ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -ge $true               ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -ige \"Jan. 1st. 2014\" ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -cge \"Jan. 1st. 2014\" ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -ge \"01/01/2014\"      ", "True")]
        [TestCase("(Get-Date \"01/01/2015\") -ge (Get-Date \"01/01/2015\")  ", "True")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -ge 1.2345              ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -ge [float] 1.2345      ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -ge [long] 1            ", "True")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -ge 1                   ", "True")]
        [TestCase("(Get-Date \"01/01/2015\") -ge [char] 0x1000              ", "True")]
        [TestCase("(Get-Date \"01/01/2015\") -ge [byte] 0x10                ", "True")]
            /// -lt, -ilt, -clt
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -lt $null               ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -lt $true               ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -ilt \"Jan. 1st. 2014\" ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -clt \"Jan. 1st. 2014\" ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -lt \"01/01/2014\"      ", "False")]
        [TestCase("(Get-Date \"01/01/2015\") -lt (Get-Date \"01/01/2015\")  ", "False")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -lt 1.2345              ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -lt [float] 1.2345      ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -lt [long] 1            ", "False")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -lt 1                   ", "False")]
        [TestCase("(Get-Date \"01/01/2015\") -lt [char] 0x1000              ", "False")]
        [TestCase("(Get-Date \"01/01/2015\") -lt [byte] 0x10                ", "False")]
            /// -le, -ile, -cle
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -le $null               ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -le $true               ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -ile \"Jan. 1st. 2014\" ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -cle \"Jan. 1st. 2014\" ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -le \"01/01/2014\"      ", "False")]
        [TestCase("(Get-Date \"01/01/2015\") -le (Get-Date \"01/01/2015\")  ", "True")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -le 1.2345              ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -le [float] 1.2345      ", "Exception")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -le [long] 1            ", "False")]
        [TestCase("$d = Get-Date \"01/01/2015\"; $d -le 1                   ", "False")]
        [TestCase("(Get-Date \"01/01/2015\") -le [char] 0x1000              ", "False")]
        [TestCase("(Get-Date \"01/01/2015\") -le [byte] 0x10                ", "False")]
        public void RelationshipOperatorsWithDateTime(string input, string expected)
        {
            try
            {
                string result = ReferenceHost.Execute(input);
                Assert.AreEqual(expected + Environment.NewLine, result);
            }
            catch (Exception)
            {
                ///
                /// Pash throws PSInvalidOperationException
                /// Powershell throws PSInvalidCastException, or RuntimeException
                ///
                Assert.AreEqual(expected, "Exception");
            }
        }


        ///
        /// RelationshipOperatorsWithNumericTypes:
        ///     - $null, string, datetime are selected based on the LHS operand
        ///     - conversion operations that are invalid result in Exceptions
        ///     - precedence:  double > float > long > int > char > byte > bool
        ///     - types can be mixed and matched
        ///     - based on type precedence, conversion takes place to normalize operands
        ///     - based on the normalization a type specific comparer is choosen to handle the operation
        ///
        [Test]
            /// -gt, -igt, -cgt
        [TestCase("1.2345 -gt $null                                         ", "True")]
        [TestCase("1.2345 -gt \"1.2345\"                                    ", "False")]
        [TestCase("1.2345 -gt \"ABC\"                                       ", "Exception")]
        [TestCase("1.2345 -gt (Get-Date \"01/01/2015\")                     ", "Exception")]
        [TestCase("1.2345 -gt $true                                         ", "True")]
        [TestCase("1.2345 -gt 1.23449                                       ", "True")]
        [TestCase("1.2345 -gt [float] 1.2345                                ", "False")]
        [TestCase("[float] 1.2345 -gt 1.2345                                ", "True")]
        [TestCase("[float] 1.2345 -gt [float] 1.23449                       ", "True")]
        [TestCase("[float] 1.2345 -gt [long] 1                              ", "True")]
        [TestCase("[long] 11 -gt [float] 1.2345                             ", "True")]
        [TestCase("[long] 11 -gt [long] 10                                  ", "True")]
        [TestCase("[long] 11 -gt 11                                         ", "False")]
        [TestCase("11 -gt [long] 10                                         ", "True")]
        [TestCase("11 -gt 10                                                ", "True")]
        [TestCase("11 -gt [char] 10                                         ", "True")]
        [TestCase("[char] 0x1000 -gt 4095                                   ", "True")]
        [TestCase("[char] 0x1000 -gt [char] 0x0FFF                          ", "True")]
        [TestCase("[char] 0x0010 -gt [byte] 0x10                            ", "False")]
        [TestCase("[byte] 0x10 -gt [char] 0x0010                            ", "False")]
        [TestCase("[byte] 0x10 -gt [byte] 0x0F                              ", "True")]
        [TestCase("[byte] 0x10 -gt $false                                   ", "True")]
            /// -ge, -ige, -cge
        [TestCase("1.2345 -ge $null                                         ", "True")]
        [TestCase("1.2345 -ge \"1.2345\"                                    ", "True")]
        [TestCase("1.2345 -ge \"ABC\"                                       ", "Exception")]
        [TestCase("1.2345 -ge (Get-Date \"01/01/2015\")                     ", "Exception")]
        [TestCase("1.2345 -ge $true                                         ", "True")]
        [TestCase("1.2345 -ge 1.23449                                       ", "True")]
        [TestCase("1.2345 -ge [float] 1.2345                                ", "False")]
        [TestCase("[float] 1.2345 -ge 1.2345                                ", "True")]
        [TestCase("[float] 1.2345 -ge [float] 1.23449                       ", "True")]
        [TestCase("[float] 1.2345 -ge [long] 1                              ", "True")]
        [TestCase("[long] 11 -ge [float] 1.2345                             ", "True")]
        [TestCase("[long] 11 -ge [long] 11                                  ", "True")]
        [TestCase("[long] 11 -ge 11                                         ", "True")]
        [TestCase("11 -ge [long] 11                                         ", "True")]
        [TestCase("11 -ge 11                                                ", "True")]
        [TestCase("11 -ge [char] 10                                         ", "True")]
        [TestCase("[char] 0x1000 -ge 4096                                   ", "True")]
        [TestCase("[char] 0x1000 -ge [char] 0x1000                          ", "True")]
        [TestCase("[char] 0x0010 -ge [byte] 0x10                            ", "True")]
        [TestCase("[byte] 0x10 -ge [char] 0x0010                            ", "True")]
        [TestCase("[byte] 0x10 -ge [byte] 0x0F                              ", "True")]
        [TestCase("[byte] 0x10 -ge $false                                   ", "True")]
            /// -lt, -ilt, -clt
        [TestCase("1.2345 -lt $null                                         ", "False")]
        [TestCase("1.2345 -lt \"1.2345\"                                    ", "False")]
        [TestCase("1.2345 -lt \"ABC\"                                       ", "Exception")]
        [TestCase("1.2345 -lt (Get-Date \"01/01/2015\")                     ", "Exception")]
        [TestCase("1.2345 -lt $true                                         ", "False")]
        [TestCase("1.2345 -lt 1.23449                                       ", "False")]
        [TestCase("1.2345 -lt [float] 1.2345                                ", "True")]
        [TestCase("[float] 1.2345 -lt 1.2345                                ", "False")]
        [TestCase("[float] 1.2345 -lt [float] 1.23449                       ", "False")]
        [TestCase("[float] 1.2345 -lt [long] 1                              ", "False")]
        [TestCase("[long] 11 -lt [float] 1.2345                             ", "False")]
        [TestCase("[long] 11 -lt [long] 12                                  ", "True")]
        [TestCase("[long] 11 -lt 11                                         ", "False")]
        [TestCase("11 -lt [long] 12                                         ", "True")]
        [TestCase("11 -lt 12                                                ", "True")]
        [TestCase("11 -lt [char] 12                                         ", "True")]
        [TestCase("[char] 0x1000 -lt 4097                                   ", "True")]
        [TestCase("[char] 0x1000 -lt [char] 0x1000                          ", "False")]
        [TestCase("[char] 0x0010 -lt [byte] 0x10                            ", "False")]
        [TestCase("[byte] 0x10 -lt [char] 0x0011                            ", "True")]
        [TestCase("[byte] 0x10 -lt [byte] 0x11                              ", "True")]
        [TestCase("[byte] 0x10 -lt $false                                   ", "False")]
            /// -le, -ile, -cle
        [TestCase("1.2345 -le $null                                         ", "False")]
        [TestCase("1.2345 -le \"1.2345\"                                    ", "True")]
        [TestCase("1.2345 -le \"ABC\"                                       ", "Exception")]
        [TestCase("1.2345 -le (Get-Date \"01/01/2015\")                     ", "Exception")]
        [TestCase("1.2345 -le $true                                         ", "False")]
        [TestCase("1.2345 -le 1.23449                                       ", "False")]
        [TestCase("1.2345 -le [float] 1.2345                                ", "True")]
        [TestCase("[float] 1.2345 -le 1.2345                                ", "False")]
        [TestCase("[float] 1.2345 -le [float] 1.23456                       ", "True")]
        [TestCase("[float] 1.2345 -le [long] 1                              ", "False")]
        [TestCase("[long] 11 -le [float] 1.2345                             ", "False")]
        [TestCase("[long] 11 -le [long] 11                                  ", "True")]
        [TestCase("[long] 11 -le 11                                         ", "True")]
        [TestCase("11 -le [long] 11                                         ", "True")]
        [TestCase("11 -le 11                                                ", "True")]
        [TestCase("11 -le [char] 11                                         ", "True")]
        [TestCase("[char] 0x1000 -le 4096                                   ", "True")]
        [TestCase("[char] 0x1000 -le [char] 0x1000                          ", "True")]
        [TestCase("[char] 0x0010 -le [byte] 0x10                            ", "True")]
        [TestCase("[byte] 0x10 -le [char] 0x0011                            ", "True")]
        [TestCase("[byte] 0x10 -le [byte] 0x10                              ", "True")]
        [TestCase("[byte] 0x10 -le $false                                   ", "False")]
        public void RelationshipOperatorsWithNumericTypes(string input, string expected)
        {
            try
            {
                string result = ReferenceHost.Execute(input);
                Assert.AreEqual(expected + Environment.NewLine, result);
            }
            catch (Exception)
            {
                ///
                /// Pash throws PSInvalidOperationException
                /// Powershell throws PSInvalidCastException, or RuntimeException
                ///
                Assert.AreEqual(expected, "Exception");
            }
        }
    }
}
