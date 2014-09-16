using System;
using System.Management;
using NUnit.Framework;
using System.Collections;

namespace ReferenceTests.Language.Operators
{
    [TestFixture]
    public class ArithmeticAssignmentTests : ReferenceTestBase
    {
        
        public void PlusEqualsWorks()
        {
            ExecuteAndCompareTypedResult("$a = 3; $a += 2.5; $a", 5.5f);
        }
        
        public void TimeEqualsWorks()
        {
            ExecuteAndCompareTypedResult("$a = 3; $a *= 2.5; $a", 7.5f);
        }

        public void MinusEqualsWorks()
        {
            ExecuteAndCompareTypedResult("$a = 5; $a -= 6.5; $a", -1.5f);
        }

        public void DivideEqualsWorks()
        {
            ExecuteAndCompareTypedResult("$a = 5; $a /= 2; $a", 2.5f);
        }

        public void RemainderEqualsWorks()
        {
            ExecuteAndCompareTypedResult("$a = 5.3; $a %= 0.4; $a", 0.2f);
        }
        
        [Test]
        public void PlusEqualsArrayConcatWorksWithOneItem()
        {
            ExecuteAndCompareTypedResult("$a = @(1,2); $a += 3; $a", 1, 2, 3);
        }

        [Test]
        public void PlusEqualsArrayConcatWorksWithMultipleItems()
        {
            ExecuteAndCompareTypedResult("$a = @(1,2); $a += '3','4'; $a", 1, 2, "3", "4");
        }

        [Test]
        public void PlusEqualsHashtableConcatWorks()
        {
            var expected = new Hashtable() {
                {"a", "1"},
                {"b", "2"},
                {"c", 3}
            };
            ExecuteAndCompareTypedResult("$a = @{a='1';b='2'}; $a += @{c=3}; $a", expected);
        }

        [Test]
        public void PlusEqualstringConcatWorks()
        {
            ExecuteAndCompareTypedResult("$a = '12'; $a += 3; $a", "123");
        }

        [Test]
        public void TimesEqualsStringReplicationWorks()
        {
            ExecuteAndCompareTypedResult("$a = 'fo'; $a *= 3; $a", "fofofo");
        }

        [Test]
        public void TimesEqualsArrayReplicationWorks()
        {
            ExecuteAndCompareTypedResult("$a = @(1,2); $a *= 3; $a", 1, 2, 1, 2, 1, 2);
        }
    }
}

