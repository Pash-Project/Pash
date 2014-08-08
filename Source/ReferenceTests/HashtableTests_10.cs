using System;
using System.Linq;
using NUnit.Framework;
using System.Collections;
using System.Runtime.InteropServices;

namespace ReferenceTests
{
    [TestFixture]
    public class HashtableTests_10 : ReferenceTestBase
    {
        private static readonly string _stdHTString = "$h1 = @{ FirstName = \"James\"; LastName = \"Anderson\"; IDNum = 123 }";
        private static readonly Hashtable _stdHT = new Hashtable() { {"FirstName", "James"}, {"LastName", "Anderson"}, {"IDNum", 123}};

        [TestCase(".firstname", "James")]
        [TestCase("[\"lastname\"]", "Anderson")]
        [TestCase(".IDNum", 123)]
        public void Access_Spec_10_1(string access, object expected)
        {
            var cmd = NewlineJoin(
                _stdHTString,
                "$h1" + access
                );
            ExecuteAndCompareTypedResult(cmd, expected);
        }

		[Test]
        public void Access_Spec_10_1_Keys()
        {
            var cmd = NewlineJoin(
                _stdHTString,
                "$h1.Keys"
                );
            ExecuteAndCompareTypedResult(cmd, _stdHT.Keys);
        }

		[Test]
        public void CanCreateEmptyHashtable_Spec_10_2()
        {
            var cmd = NewlineJoin("$h1 = @{}", "$h1.GetType().FullName", "$h1.Count");
            ExecuteAndCompareTypedResult(cmd, typeof(Hashtable).FullName, 0);
        }

        [TestCase(".Dept = \"Finance\"", ".Dept", "Finance")]
        [TestCase("[\"Salaried\"] = $false", ".Salaried", false)]
        [TestCase(".Remove(\"FirstName\")", ".FirstName", null)]
        public void CanAddAndRemove_Spec_10_3(string action, string check, object expected)
        {
            var cmd = NewlineJoin(
                _stdHTString,
                "$h1" + action,
                "$h1" + check
                );
            ExecuteAndCompareTypedResult(cmd, expected);
        }

        [Test]
        public void IsReferenceType_Spec_10_5()
        {
            var cmd = NewlineJoin(
                _stdHTString,
                "$h2 = $h1",
                "$h1.Firstname = \"John\"",
                "$h1.Firstname",
                "$h2.Firstname"
                );
            ExecuteAndCompareTypedResult(cmd, "John", "John");
        }

        [Test]
        public void CanEnumerate_Spec_10_6()
        {
            var cmd = NewlineJoin(
                _stdHTString,
                "foreach($e in $h1.Keys) { \"Key is \" + $e + \", ValueType is \" + $h1[$e] }"
            );
            string[] expected = new string[3];
            int i = 0;
            foreach (var key in _stdHT)
            {
                expected[i++] = "Key is " + key + ", ValueType is " + _stdHT[key];
            }
            ExecuteAndCompareTypedResult(cmd, expected);
        }

    }
}

