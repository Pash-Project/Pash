using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReferenceTests.Language.Operators
{
    [TestFixture]
    public class RangeOperatorTests_7_4 : ReferenceTestBase
    {
        [Test]
        public void Range_Spec_7_4_Normal()
        {
            ExecuteAndCompareTypedResult("1..10", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        }

        [Test]
        public void Range_Spec_7_4_Negative()
        {
            ExecuteAndCompareTypedResult("-500..-495", -500, -499, -498, -497, -496, -495);
        }

        [Test]
        public void Range_Spec_7_4_SingleElement()
        {
            ExecuteAndCompareTypedResult("16..16", 16);
        }

        [Test]
        public void Range_Spec_7_4_DoubleDecimal()
        {
            ExecuteAndCompareTypedResult("$x=1.5; $x..5.40D", 2, 3, 4, 5);
        }

        [Test]
        public void Range_Spec_7_4_True()
        {
            ExecuteAndCompareTypedResult("$true..3", 1, 2, 3);
        }

        [Test]
        public void Range_Spec_7_4_Null()
        {
            ExecuteAndCompareTypedResult("-2..$null", -2, -1, 0);
        }

        [Test]
        public void Range_Spec_7_4_HexStrings()
        {
            ExecuteAndCompareTypedResult("\"0xf\"..\"0xa\"", 15, 14, 13, 12, 11, 10);
        }
    }
}
