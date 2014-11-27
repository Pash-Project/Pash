using System;
using NUnit.Framework;

namespace ReferenceTests.Operators
{
    [TestFixture]
    public class PostfixIncrementDecrementOperatorTests_7_1_5 : ReferenceTestBase
    {
        [TestCase("$i = 0; $j = $i++; $j; $i", 0, 1)] // increments properly by 1, after assignment
        [TestCase("$i = 0; ($i++); $i", 0, 1)] // parenthesis cause writing sideffects to pipeline
        [TestCase("$i = $null; $j = $i++; $j; $i", null, 1)] // with $null value
        [TestCase("$j = $i++; $j; $i", null, 1)] // not defined before
        [TestCase("$i = 0.1; $j = $i++; $j; $i", 0.1d, 1.1d)] // double
        public void PostfixIncrement(string cmd, object assigned, object incremented)
        {
            ExecuteAndCompareTypedResult(cmd, assigned, incremented);
        }

        [Test]
        public void PostfixIncrementDecimal()
        {
            ExecuteAndCompareTypedResult("$i = 0.1D; $j = $i++; $j; $i", 0.1m, 1.1m);
        }

        [TestCase("$i = 0; $j = $i--; $j; $i", 0, -1)] // increments properly by 1, after assignment
        [TestCase("$i = 0; ($i--); $i", 0, -1)] // parenthesis cause writing sideffects to pipeline
        [TestCase("$i = $null; $j = $i--; $j; $i", null, -1)] // with $null value
        [TestCase("$j = $i--; $j; $i", null, -1)] // not defined before
        [TestCase("$i = 0.1; $j = $i--; $j; $i", 0.1d, -0.9d)] // double
        public void PostfixDecrement(string cmd, object assigned, object incremented)
        {
            ExecuteAndCompareTypedResult(cmd, assigned, incremented);
        }

        [Test]
        public void PostfixDerementDecimal()
        {
            ExecuteAndCompareTypedResult("$i = 0.1D; $j = $i--; $j; $i", 0.1m, -0.9m);
        }

        [Test]
        public void PostfixIncrementChangesDatatypeFromIntToDoubleIfValueTooHigh()
        {
            int maxint = Int32.MaxValue;
            ExecuteAndCompareTypedResult("$i = [int]::MaxValue; $j = $i++; $j; $i", maxint, ((double)maxint) + 1);
        }

        [Test]
        public void PostfixIncrementChangesDatatypeFromLongToDoubleIfValueTooHigh()
        {
            long maxlong = Int64.MaxValue;
            ExecuteAndCompareTypedResult("$i = [long]::MaxValue; $j = $i++; $j; $i", maxlong, ((double)maxlong) + 1);
        }

        [Test]
        public void PostfixDecrementChangesDatatypeFromIntToDoubleIfValueTooLow()
        {
            int minint = Int32.MinValue;
            ExecuteAndCompareTypedResult("$i = [int]::MinValue; $j = $i--; $j; $i", minint, ((double)minint) - 1);
        }

        [Test]
        public void PostfixDecrementChangesDatatypeFromLongToDoubleIfValueTooLow()
        {
            long minlong = Int64.MinValue;
            ExecuteAndCompareTypedResult("$i = [long]::MinValue; $j = $i--; $j; $i", minlong, ((double)minlong) - 1);
        }

        [Test]
        public void PostfixIncrementDecrement_Spec_Example1()
        {
            ExecuteAndCompareTypedResult("$i = 0; $i++; $j = $i--; $j; $i;", 1, 0);
        }

        [Test]
        public void PostfixIncrementDecrement_Spec_Example2()
        {
            var cmd = NewlineJoin(
                "$a = 1,2,3",
                "$b = 9,8,7",
                "$i = 0",
                "$j = 1",
                "$b[$j--] = $a[$i++]",
                "$a; $b; $i; $j");
            ExecuteAndCompareTypedResult(cmd, new int[] {1,2,3}, new int[] {9,1,7}, 1, 0);
        }

        [Test]
        public void PostfixIncrementDecrementInSameLine()
        {
            var cmd = NewlineJoin(
                "$a = 1,2,3",
                "$b = 9,8,7",
                "$i = 0",
                "$b[$i++] = $a[$i++]",
                "$a; $b; $i");
            ExecuteAndCompareTypedResult(cmd, new int[] {1,2,3}, new int[] {9,1,7}, 2);
        }
        // TODO: tests with type constraint as "[int]$x = [int]::MaxValue; $x++"
    }
}

