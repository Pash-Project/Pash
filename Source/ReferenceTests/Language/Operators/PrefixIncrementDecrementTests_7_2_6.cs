using System;
using NUnit.Framework;

namespace ReferenceTests.Operators
{
    public class PrefixIncrementDecrementTests_7_2_6 : ReferenceTestBase
    {
        [TestCase("$i = 0; $j = ++$i; $j; $i", 1, 1)] // increments properly by 1, before assignment
        [TestCase("$i = 0; (++$i); $i", 1, 1)] // parenthesis cause writing sideffects to pipeline
        [TestCase("$i = $null; $j = ++$i; $j; $i", 1, 1)] // with $null value
        [TestCase("$j = ++$i; $j; $i", 1, 1)] // not defined before
        [TestCase("$i = 0.1; $j = ++$i; $j; $i", 1.1d, 1.1d)] // double
        public void PrefixIncrement(string cmd, object assigned, object incremented)
        {
            ExecuteAndCompareTypedResult(cmd, assigned, incremented);
        }

        [Test]
        public void PrefixIncrementDecimal()
        {
            ExecuteAndCompareTypedResult("$i = 0.1D; $j = ++$i; $j; $i", 1.1m, 1.1m);
        }

        [TestCase("$i = 0; $j = --$i; $j; $i", -1, -1)] // increments properly by 1, before assignment
        [TestCase("$i = 0; (--$i); $i", -1, -1)] // parenthesis cause writing sideffects to pipeline
        [TestCase("$i = $null; $j = --$i; $j; $i", -1, -1)] // with $null value
        [TestCase("$j = --$i; $j; $i", -1, -1)] // not defined before
        [TestCase("$i = 0.1; $j = --$i; $j; $i", -0.9d, -0.9d)] // double
        public void PrefixDecrement(string cmd, object assigned, object incremented)
        {
            ExecuteAndCompareTypedResult(cmd, assigned, incremented);
        }

        [Test]
        public void PrefixDerementDecimal()
        {
            ExecuteAndCompareTypedResult("$i = 0.1D; $j = --$i; $j; $i", -0.9m, -0.9m);
        }

        [Test]
        public void PostfixIncrementChangesDatatypeFromIntToDoubleIfValueTooHigh()
        {
            double maxintp1 = ((double)Int32.MaxValue) + 1;
            ExecuteAndCompareTypedResult("$i = [int]::MaxValue; $j = ++$i; $j; $i", maxintp1, maxintp1);
        }

        [Test]
        public void PrefixIncrementChangesDatatypeFromLongToDoubleIfValueTooHigh()
        {
            double maxlongp1 = ((double)Int64.MaxValue) + 1;
            ExecuteAndCompareTypedResult("$i = [long]::MaxValue; $j = ++$i; $j; $i", maxlongp1, maxlongp1);
        }

        [Test]
        public void PrefixDecrementChangesDatatypeFromIntToDoubleIfValueTooLow()
        {
            double minintm1 = ((double)Int32.MinValue) - 1;
            ExecuteAndCompareTypedResult("$i = [int]::MinValue; $j = --$i; $j; $i", minintm1, minintm1);
        }

        [Test]
        public void PrefixDecrementChangesDatatypeFromLongToDoubleIfValueTooLow()
        {
            double minlongm1 = ((double)Int64.MinValue) - 1;
            ExecuteAndCompareTypedResult("$i = [long]::MinValue; $j = --$i; $j; $i", minlongm1, minlongm1);
        }

        [Test]
        public void PrefixIncrementDecrement_Spec_Example1()
        {
            ExecuteAndCompareTypedResult("$i = 0; ++$i; $j = --$i; $j; $i;", 0, 0);
        }

        [Test]
        public void PrefixIncrementDecrement_Spec_Example2()
        {
            var cmd = NewlineJoin(
                "$a = 1,2,3",
                "$b = 9,8,7",
                "$i = 0",
                "$j = 1",
                "$b[--$j] = $a[++$i]",
                "$a; $b; $i; $j");
            ExecuteAndCompareTypedResult(cmd, new int[] {1,2,3}, new int[] {2,8,7}, 1, 0);
        }

        [Test]
        public void PrefixIncrementDecrementInSameLine()
        {
            var cmd = NewlineJoin(
                "$a = 1,2,3",
                "$b = 9,8,7",
                "$i = 0",
                "$b[++$i] = $a[++$i]",
                "$a; $b; $i");
            ExecuteAndCompareTypedResult(cmd, new int[] {1,2,3}, new int[] {9,8,2}, 2);
        }
        // TODO: tests with type constraint as "[int]$x = [int]::MaxValue; $x++"
    }
}

