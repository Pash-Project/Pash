using System;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Language
{
    public class Loops : ReferenceTestBase
    {
        [Test]
        public void For()
        {
            var cmd = "for ($i = 0; $i -ile 5; $i++) { $i }";
            ExecuteAndCompareTypedResult(cmd, 0, 1, 2, 3, 4, 5);
        }

        [Test]
        public void ForWithNonBoolCondition()
        {
            // the condition is always a substring from foo: "foo", "oo", "o", "". Last should be $false
            var cmd = "for ($i = 0; 'foo'.Substring($i); $i++) { $i }";
            ExecuteAndCompareTypedResult(cmd, 0, 1, 2);
        }

        [Test]
        public void While()
        {
            /* Should behave exactly like the for-loop */
            var cmd = "$i = 0; while ($i -ile 5) { $i; $i++ }";
            ExecuteAndCompareTypedResult(cmd, 0, 1, 2, 3, 4, 5);
        }

        [Test]
        public void WhileWithNonBoolCondition()
        {
            // the condition is always a substring from foo: "foo", "oo", "o", "". Last should be $false
            var cmd = "$i = 0; while ('foo'.Substring($i)) { $i; $i++ }";
            ExecuteAndCompareTypedResult(cmd, 0, 1, 2);
        }

        [Test]
        public void DoWhile()
        {
            /* Should behave exactly like the for-loop */
            var cmd = "$i = 0; do { $i; $i++ } while ($i -ile 5)";
            ExecuteAndCompareTypedResult(cmd, 0, 1, 2, 3, 4, 5);
        }

        [Test]
        public void DoWhileWithNonBoolCondition()
        {
            var cmd = "$i = 0; do { $i; $i++ } while ($null); $i";
            ExecuteAndCompareTypedResult(cmd, 0, 1);
        }

        [Test]
        public void DoUntil()
        {
            /* Should behave exactly like the do-while loop with inverted condition */
            var cmd = "$i = 0; do { $i; $i++ } until ($i -igt 5)";
            ExecuteAndCompareTypedResult(cmd, 0, 1, 2, 3, 4, 5);
        }

        [Test]
        public void DoUntilWithNonBoolCondition()
        {
            /* Should behave exactly like the do-while loop with inverted condition */
            var cmd = "$i = 0; do { $i; $i++ } until (4); $i";
            ExecuteAndCompareTypedResult(cmd, 0, 1);
        }

        [Test]
        public void ForLoopWithAssignmentStatementAsBodyShouldNotOutputAssignmentResultOnEachIteration()
        {
            var cmd = "$j = 0; for ($i = 0; $i -ile 10; $i++) { $j++ }; $j";
            ExecuteAndCompareTypedResult(cmd, 11);
        }

        [Test]
        public void ForEach()
        {
            string cmd = "foreach ($i in (0..5)) { $i }";
            ExecuteAndCompareTypedResult(cmd, 0, 1, 2, 3, 4, 5);
        }

        [Test]
        public void ForEachWithAssignmentStatementAsBodyShouldNotOutputAssignmentResultOnEachIteration()
        {
            string cmd = "$j = 0; foreach ($i in 0..10) { $j++ }; $j";
            ExecuteAndCompareTypedResult(cmd, 11);
        }

        [Test]
        public void ForEachCharacterInStringIsString()
        {
            var cmd = "foreach ($char in 'abc') { $char }";
            ExecuteAndCompareTypedResult(cmd, "abc");
        }

        [Test]
        public void ForEachCharacterInArray()
        {
            var cmd = "foreach ($char in 'abc'.ToCharArray()) { $char }";
            ExecuteAndCompareTypedResult(cmd, 'a', 'b', 'c');
        }
    }
}

