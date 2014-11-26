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
            var result = ReferenceHost.Execute("for ($i = 0; $i -ile 5; $i++) { $i }");
            Assert.AreEqual(NewlineJoin("0", "1", "2", "3", "4", "5"), result);
        }

        [Test]
        public void While()
        {
            /* Should behave exactly like the for-loop */
            var result = ReferenceHost.Execute("$i = 0; while ($i -ile 5) { $i; $i++ }");
            Assert.AreEqual(NewlineJoin("0", "1", "2", "3", "4", "5"), result);
        }

        [Test]
        public void ForLoopWithAssignmentStatementAsBodyShouldNotOutputAssignmentResultOnEachIteration()
        {
            string result = ReferenceHost.Execute("$j = 0; for ($i = 0; $i -ile 10; $i++) { $j++ }; $j");

            Assert.AreEqual(NewlineJoin("11"), result);
        }

        [Test]
        public void ForEach()
        {
            string result = ReferenceHost.Execute("foreach ($i in (0..5)) { $i }");
            Assert.AreEqual(NewlineJoin("0", "1", "2", "3", "4", "5"), result);
        }

        [Test]
        public void ForEachWithAssignmentStatementAsBodyShouldNotOutputAssignmentResultOnEachIteration()
        {
            string result = ReferenceHost.Execute("$j = 0; foreach ($i in 0..10) { $j++ }; $j");

            Assert.AreEqual(NewlineJoin("11"), result);
        }

        [Test]
        public void ForEachCharacterInStringIsString()
        {
            string result = ReferenceHost.Execute("foreach ($char in 'abc') { $char }");

            Assert.AreEqual(NewlineJoin("abc"), result);
        }

        [Test]
        public void ForEachCharacterInArray()
        {
            string result = ReferenceHost.Execute("foreach ($char in 'abc'.ToCharArray()) { $char }");

            Assert.AreEqual(NewlineJoin("a", "b", "c"), result);
        }
    }
}

