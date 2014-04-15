using System;
using NUnit.Framework;
using System.Management.Automation;
using TestPSSnapIn;

namespace ReferenceTests
{
    [TestFixture]
    public class ObjectCommandTests : ReferenceTestBase
    {

        [SetUp]
        public void SetUp()
        {
            ImportTestCmdlets();
        }

        [TearDown]
        public void TearDown()
        {
            CleanImports();
        }

        [Test]
        public void NewObjectCanCreatePSObject()
        {
            var results = ReferenceHost.RawExecute("New-Object -Type PSObject");
            results.ShouldNotBeEmpty();
            var obj = results[0].BaseObject;
            Assert.True(obj is PSCustomObject);
        }

        [Test]
        public void AddMemberCanAddNoteProperties()
        {
            var results = ReferenceHost.RawExecute(NewlineJoin(new string[] {
                "$a = New-Object -Type PSObject",
                "$a | Add-Member -Type NoteProperty -Name TestName -Value TestValue",
                "$a"
            }));
            results.ShouldNotBeEmpty();
            var obj = results[0];
            Assert.NotNull(obj.Members["TestName"]);
            Assert.NotNull(obj.Properties["TestName"]);
        }

        [Test]
        public void CustomPSObjectPropertiesCanBeAccessedCaseInsensitive()
        {
            var result = ReferenceHost.Execute(NewlineJoin(new string[] {
                "$a = New-Object -Type PSObject",
                "$a | Add-Member -Type NoteProperty -Name TestName -Value TestValue",
                "$a.testname"
            }));
            Assert.AreEqual("TestValue" + Environment.NewLine, result);
        }

        [Test]
        public void AccessingNonExistingPropertiesDoesntFail()
        {
            var result = ReferenceHost.Execute(NewlineJoin(new string[] {
                "$a = New-Object -Type PSObject",
                "$a.testname"
            }));
            Assert.AreEqual(Environment.NewLine, result);
        }

        [Test]
        public void CanGetCustomCSharpObjectAndIdentifyType()
        {
            var result = ReferenceHost.Execute(NewlineJoin(new string[] {
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)),
                "$a.GetType().FullName"
            }));
            Assert.AreEqual(typeof(CustomTestClass).FullName + Environment.NewLine, result);
        }

        [Test]
        public void CanAccessCSharpObjectProperty()
        {
            var result = ReferenceHost.Execute(NewlineJoin(new string[] {
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$a.MessageProperty"
            }));
            Assert.AreEqual("foo" + Environment.NewLine, result);
        }

        [Test]
        public void CanSetCSharpObjectProperty()
        {
            var result = ReferenceHost.Execute(NewlineJoin(new string[] {
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$a.MessageProperty = 'baz'",
                "$a.MessageProperty"
            }));
            Assert.AreEqual("baz" + Environment.NewLine, result);
        }

        [Test]
        public void CanAccessCSharpObjectField()
        {
            var result = ReferenceHost.Execute(NewlineJoin(new string[] {
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$a.MessageField"
            }));
            Assert.AreEqual("bar" + Environment.NewLine, result);
        }

        [Test]
        public void CanSetCSharpObjectField()
        {
            var result = ReferenceHost.Execute(NewlineJoin(new string[] {
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$a.MessageField = 'baz'",
                "$a.MessageField"
            }));
            Assert.AreEqual("baz" + Environment.NewLine, result);
        }

        [Test]
        public void CanInvokeCSharpObjectMethodAndGetResult()
        {
            var result = ReferenceHost.Execute(NewlineJoin(new string[] {
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$b = $a.Combine()",
                "$b.GetType().FullName",
                "$b",
            }));
            var expected = NewlineJoin(new string[] {
                typeof(string).FullName,
                "foobar"
            });
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CanInvokeCSharpObjectMethodWithArguments()
        {
            var result = ReferenceHost.Execute(NewlineJoin(new string[] {
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$a.SetMessages('bla', 'blub')",
                "$a.MessageProperty",
                "$a.MessageField",
            }));
            var expected = NewlineJoin(new string[] {
                "bla",
                "blub"
            });
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CanGetCSharpObjectMethodAndInvokeLater()
        {
            var result = ReferenceHost.Execute(NewlineJoin(new string[] {
                "$a = " + CmdletName(typeof(TestCreateCustomObjectCommand)) + " 'foo' 'bar'",
                "$b = $a.SetMessages",
                "$c = $a.Combine",
                "$b.Invoke('bla', 'blub')",
                "$c.Invoke()"
            }));
            Assert.AreEqual("blablub" + Environment.NewLine, result);
        }
    }
}

