using System;
using NUnit.Framework;
using System.Management.Automation;

namespace ReferenceTests
{
    public class ArrayTests : ReferenceTestBase
    {

        [TestCase("1,2,3")]
        [TestCase("@('foo',2,3)")]
        public void ArrayExpressionIsArrayType(string definition)
        {
            var cmd = String.Format("$a = {0}; $a.GetType().FullName", definition);
            var result = ReferenceHost.Execute(cmd);
            Assert.AreEqual(typeof(object[]).FullName + Environment.NewLine, result);
        }

        [Test]
        public void ArrayInVariableGetsEvaluatedWhenPassedToPipeline()
        {
            var cmd = String.Format("$a = @(1,2,3); $a");
            var results = ReferenceHost.RawExecute(cmd);
            Assert.AreEqual(3, results.Count);
        }

        [Test]
        public void NestedOneDimensionalArrayEvaluates()
        {
            var results = ReferenceHost.RawExecute("@(@(@('foo')))");
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(PSObject.AsPSObject("foo"), results[0]);
        }



        [Test]
        public void NestedTwoDimensionalArrayWorks()
        {
            var results = ReferenceHost.RawExecute("@(@(@('foo')), @(@('bar')))");
            Assert.AreEqual(2, results.Count);
            var expected = new[] { "foo", "bar" };
            for (int i = 0; i < 2; i++)
            {
                var psobjSubarray = results[i];
                Assert.IsInstanceOf<PSObject>(psobjSubarray);
                var subarray = ((PSObject)psobjSubarray).BaseObject as object[];
                Assert.NotNull(subarray);
                Assert.AreEqual(PSObject.AsPSObject(expected[i]), subarray[0]);

            }
        }
    }
}

