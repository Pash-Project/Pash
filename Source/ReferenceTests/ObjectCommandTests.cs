using System;
using NUnit.Framework;
using System.Management.Automation;
using TestPSSnapIn;
using System.Runtime.Remoting;

namespace ReferenceTests
{
    [TestFixture]
    public class ObjectCommandTests : ReferenceTestBase
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
        public void AddMemberCanAddNoteProperties()
        {
            var results = ReferenceHost.RawExecute(NewlineJoin(
                "$a = New-Object -Type PSObject",
                "$a | Add-Member -Type NoteProperty -Name TestName -Value TestValue",
                "$a"
            ));
            Assert.AreEqual(1, results.Count, "No results");
            var obj = results[0];
            Assert.NotNull(obj.Members["TestName"]);
            Assert.NotNull(obj.Properties["TestName"]);
        }

        [Test]
        public void CustomPSObjectPropertiesCanBeAccessedCaseInsensitive()
        {
            var result = ReferenceHost.Execute(NewlineJoin(
                "$a = New-Object -Type PSObject",
                "$a | Add-Member -Type NoteProperty -Name TestName -Value TestValue",
                "$a.testname"
            ));
            Assert.AreEqual(NewlineJoin("TestValue"), result);
        }

        [Test]
        public void AccessingNonExistingPropertiesDoesntFail()
        {
            var result = ReferenceHost.Execute(NewlineJoin(
                "$a = New-Object -Type PSObject",
                "$a.testname"
            ));
            Assert.AreEqual(NewlineJoin(), result);
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
        public void CanGetCustomCSharpObjectAndIdentifyType()
        {
            var result = ReferenceHost.Execute(NewlineJoin(
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)),
                "$a.GetType().FullName"
            ));
            Assert.AreEqual(NewlineJoin(typeof(CustomTestClass).FullName), result);
        }

        [Test]
        public void CanAccessCSharpObjectProperty()
        {
            var result = ReferenceHost.Execute(NewlineJoin(
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$a.MessageProperty"
            ));
            Assert.AreEqual(NewlineJoin("foo"), result);
        }

        [Test]
        public void CanSetCSharpObjectProperty()
        {
            var result = ReferenceHost.Execute(NewlineJoin(
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$a.MessageProperty = 'baz'",
                "$a.MessageProperty"
            ));
            Assert.AreEqual(NewlineJoin("baz"), result);
        }

        [Test]
        public void CanAccessCSharpObjectField()
        {
            var result = ReferenceHost.Execute(NewlineJoin(
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$a.MessageField"
            ));
            Assert.AreEqual(NewlineJoin("bar"), result);
        }

        [Test]
        public void CanSetCSharpObjectField()
        {
            var result = ReferenceHost.Execute(NewlineJoin(
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$a.MessageField = 'baz'",
                "$a.MessageField"
            ));
            Assert.AreEqual(NewlineJoin("baz"), result);
        }

        [Test]
        public void CanInvokeCSharpObjectMethodAndGetResult()
        {
            var result = ReferenceHost.Execute(NewlineJoin(
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$b = $a.Combine()",
                "$b.GetType().FullName",
                "$b"
            ));
            var expected = NewlineJoin(typeof(string).FullName, "foobar");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CanInvokeCSharpObjectMethodWithArguments()
        {
            var result = ReferenceHost.Execute(NewlineJoin(
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$a.SetMessages('bla', 'blub')",
                "$a.MessageProperty",
                "$a.MessageField"
            ));
            var expected = NewlineJoin("bla", "blub");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CanGetCSharpObjectMethodAndInvokeLater()
        {
            var result = ReferenceHost.Execute(NewlineJoin(
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$b = $a.SetMessages",
                "$c = $a.Combine",
                "$b.Invoke('bla', 'blub')",
                "$c.Invoke()"
            ));
            Assert.AreEqual(NewlineJoin("blablub"), result);
        }

        [Test]
        public void PSObjectIsntCopiedAndPropertyIsUpdatable()
        {
            var result = ReferenceHost.Execute(NewlineJoin(
                "$a = new-object psobject -property @{foo='a';bar='b';baz='c'}",
                "$b = $a",
                "$a.baz",
                "$b.baz",
                "$b.baz='d'",
                "$a.baz",
                "$b.baz"
            ));
            Assert.AreEqual(NewlineJoin("c", "c", "d", "d"), result);
        }
    }
}

