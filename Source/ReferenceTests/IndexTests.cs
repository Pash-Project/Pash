using System;
using NUnit.Framework;
using System.Management;

namespace ReferenceTests
{
    [TestFixture]
    public class IndexTests : ReferenceTestBase
    {
        [TestCase("0", "a")]
        [TestCase("\"1\"", "b")]
        [TestCase("\"0x1\"", "b")]
        [TestCase("1.2", "b")]
        [TestCase("1.6d", "c")]
        [TestCase("1+1", "c")]
        [TestCase("-2", "b")]
        [TestCase("-1", "c")]
        [TestCase("20", null)]
        public void ArrayIndexingWorks(string idxExp, string expected)
        {
            var cmd = String.Format(NewlineJoin(
                "$a = @('a', 'b', 'c')",
                "$a[{0}]"), idxExp);
            var result = ReferenceHost.Execute(cmd);
            expected = expected == null ? "" : NewlineJoin(expected);
            Assert.AreEqual(expected, result);
        }

        [TestCase("0", "a")]
        [TestCase("\"1\"", "b")]
        [TestCase("\"0x1\"", "b")]
        [TestCase("1.2", "b")]
        [TestCase("1.6d", "c")]
        [TestCase("1+1", "c")]
        [TestCase("-2", "b")]
        [TestCase("-1", "c")]
        public void ArrayIndexingWorksWithAssignment(string idxExp, string value)
        {
            var cmd = String.Format(NewlineJoin(
                "$a = @('x', 'y', 'z')",
                "$a[{0}] = '{1}'",
                "$a[{0}]"), idxExp, value);
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(value), result);
        }

        [Test]
        public void ArrayAssignmentWithTooHighIndexThrows()
        {
            // TODO: check for correct error
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate {
                ReferenceHost.Execute("$a=@(1,2,3);$a[3]=4");
            });
        }

        [TestCase("@(1,-3,4,-4,2)", new [] {"y", "x", "z"})]
        [TestCase("0,1", new [] {"x", "y"})]
        public void ArrayCanBeIndexedWithArray(string idxExp, string[] expected)
        {
            var cmd = String.Format(NewlineJoin(
                "$a = @('x', 'y', 'z')",
                "$a[{0}]"), idxExp);
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(expected), result);
        }

        [TestCase("1,0", "30")]
        [TestCase("@(0,1)", "20")]
        [TestCase("1,\"1\"", "40")]
        [TestCase("1,\"0x1\"", "40")]
        [TestCase("-1,-2", "30")]
        [TestCase("0.9,1", "40")]
        [TestCase("@(6,7)", null)]
        public void MultidimensionalArrayIndexingWorks(string idxExp, string expected)
        {
            var cmd = String.Format(NewlineJoin(
                "$b = (New-Object 'system.int32[,]' 2,2)",
                "$b[0,0] = 10",
                "$b[0,1] = 20",
                "$b[1,0] = 30",
                "$b[1,1] = 40",
                "$b[{0}]"), idxExp);
            expected = expected == null ? "" : NewlineJoin(expected);
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(expected, result);
        }

        [TestCase("1,0", "50")]
        [TestCase("1,\"1\"", "5")]
        [TestCase("1,\"0x1\"", "5")]
        [TestCase("0.9,1", "3")]
        [TestCase("@(0,1)", "10")]
        [TestCase("@(-1,-1)", "10")]
        public void MultidimensionalArrayIndexingWorksWithAssignment(string idxExp, string value)
        {
            var cmd = String.Format(NewlineJoin(
                "$b = (New-Object 'system.int32[,]' 2,2)",
                "$b[0,0] = 10",
                "$b[0,1] = 20",
                "$b[1,0] = 30",
                "$b[1,1] = 40",
                "$b[{0}] = {1}",
                "$b[{0}]"), idxExp, value);
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(value), result);
        }

        [TestCase("0")]
        [TestCase("0,1,2")]
        public void MultidimensionalArrayWrongIndexingThrows(string idxExp)
        {
            var cmd = String.Format(NewlineJoin(
                "$b = (New-Object 'system.int32[,]' 2,2)",
                "$b[0,0] = 10",
                "$b[0,1] = 20",
                "$b[1,0] = 30",
                "$b[1,1] = 40",
                "$b[{0}]"), idxExp);
            // TODO: check for correct error
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate {
                ReferenceHost.Execute(cmd);
            });
        }
       
        [TestCase("'a'", "1")]
        [TestCase("$idx", "2")]
        [TestCase("'fpp'", null)]
        public void IndexingHashtablesWorks(string idxExp, string expected)
        {
            var cmd = String.Format(NewlineJoin(
                "$idx='b';",
                "$b = @{{a='1';b='2'}}",
                "$b[{0}]"), idxExp);
            var result = ReferenceHost.Execute(cmd);
            expected = expected == null ? "" : NewlineJoin(expected);
            Assert.AreEqual(expected, result);
        }

        [TestCase("@('a', 3, 'c', -1, $idx)", new [] {"foo", "bar"})]
        [TestCase("'a',,'b'", new [] {"foo"})] // second element is an array!
        public void HashtableCanBeIndexedWithArray(string idxExp, string[] expected)
        {
            var cmd = String.Format(NewlineJoin(
                "$idx='b';",
                "$b = @{{a='foo';b='bar'}}",
                "$b[{0}]"), idxExp);
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(expected), result);
        }

        [TestCase("'a'", "5")]
        [TestCase("$idx", "6")]
        [TestCase("'fpp'", "6")]
        public void IndexingHashtablesWorksWithAssignment(string idxExp, string value)
        {
            var cmd = String.Format(NewlineJoin(
                "$idx='b';",
                "$b = @{{a='1';b='2'}}",
                "$b[{0}] = {1}",
                "$b[{0}]"), idxExp, value);
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(value), result);
        }

        [TestCase("0", "a")]
        [TestCase("-1", "c")]
        [TestCase("0x1", "b")]
        public void IndexingStringsWorks(string idxExp, string expected)
        {
            var cmd = String.Format(NewlineJoin(
                "$a = 'abc';",
                "$a[{0}]"), idxExp);
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(expected), result);
        }

        [TestCase("@(1,-3,4,-4,2)", new [] {"y", "x", "z"})]
        [TestCase("0,1", new [] {"x", "y"})]
        public void StringCanBeIndexedWithArray(string idxExp, string[] expected)
        {
            var cmd = String.Format(NewlineJoin(
                "$a = 'xyz'",
                "$a[{0}]"), idxExp);
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(expected), result);
        }
    }
}

