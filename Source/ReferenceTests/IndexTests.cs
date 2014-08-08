using System;
using NUnit.Framework;

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
        [TestCase("20", "")]
        public void ArrayIndexingWorks(string idxExp, string expected)
        {
            var cmd = String.Format(NewlineJoin(
                "$a = @('a', 'b', 'c')",
                "$a[{0}]"), idxExp);
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(expected), result);
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
        public void ArrayIndexingWithTooHighIndexThrows()
        {
            // TODO: check for correct error
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate {
                ReferenceHost.Execute("$a=@(1,2,3);$a[3]=4");
            });
        }

        [TestCase("1,0", "30")]
        [TestCase("@(0,1)", "20")]
        [TestCase("1,\"1\"", "40")]
        [TestCase("1,\"0x1\"", "40")]
        [TestCase("0.9,1", "40")]
        [TestCase("@(6,7)", "")]
        public void MultidimensionalArrayIndexingWorks(string idxExp, string expected)
        {
            var cmd = String.Format(NewlineJoin(
                "$b = (New-Object 'system.int32[,]' 2,2)",
                "$b[0,0] = 10",
                "$b[0,1] = 20",
                "$b[1,0] = 30",
                "$b[1,1] = 40",
                "$b[{0}]"), idxExp);
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(NewlineJoin(expected), result);
        }

        [TestCase("1,0", "50")]
        [TestCase("1,\"1\"", "5")]
        [TestCase("1,\"0x1\"", "5")]
        [TestCase("0.9,1", "3")]
        [TestCase("@(0,1)", "10")]
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
        [TestCase("-1,-1")] // negative indexes don't work with multiple dimensions
        public void WrongIndexingThrows(string idxExp)
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
        [TestCase("'fpp'", "")]
        public void IndexingHashtablesWorks(string idxExp, string expected)
        {
            var cmd = String.Format(NewlineJoin(
                "$idx='b';",
                "$b = @{{a='1';b='2'}}",
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
    }
}

