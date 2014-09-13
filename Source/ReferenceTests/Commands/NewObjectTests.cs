using System;
using NUnit.Framework;
using System.Management.Automation;
using TestPSSnapIn;
using System.Runtime.Remoting;
using System.Collections.Generic;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class NewObjectTests : ReferenceTestBase
    {
        [Test]
        public void NewObjectCanCreatePSObject()
        {
            var results = ReferenceHost.RawExecute("New-Object -Type PSObject");
            Assert.AreEqual(1, results.Count, "No results");
            var obj = results[0].BaseObject;
            Assert.True(obj is PSCustomObject);
        }

        [Test]
        public void NewObjectWithAcceleratedArrayType()
        {
            var result = ReferenceHost.Execute("(new-object 'regex[]' 4).GetType().GetElementType().FullName");
            Assert.AreEqual(NewlineJoin(typeof(System.Text.RegularExpressions.Regex).FullName), result);
        }

        [Test]
        public void CreatePSObjectWithPropertiesByHashtable()
        {
            var result = ReferenceHost.Execute(NewlineJoin(new string[] {
                "$obj = new-object psobject -property @{foo='abc'; bar='def'}",
                "$obj.FoO",
                "$obj.bAR"
            }));
            var expected = NewlineJoin(new string[] { "abc", "def" });
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CreatePSObjectWithPropertiesAreRawObjects()
        {
            var expected = new Dictionary<string, object>() {{"foo", "abc"}, {"bar", 3}};
            var results = ReferenceHost.RawExecute(NewlineJoin(new string[] {
                "new-object psobject -property @{foo='abc'; bar=3}"
            }));
            Assert.AreEqual(1, results.Count);
            var res = results[0];
            foreach (var key in expected.Keys)
            {
                Assert.AreEqual(expected[key], res.Properties[key].Value);
            }
        }
    }
}

