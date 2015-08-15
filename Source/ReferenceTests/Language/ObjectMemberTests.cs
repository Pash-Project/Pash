using System;
using NUnit.Framework;
using System.Management.Automation;
using TestPSSnapIn;
using System.Runtime.Remoting;
using System.Collections.Generic;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class ObjectMemberTests : ReferenceTestBaseWithTestModule
    {
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
        public void AccessingMemberOfNullThrows()
        {
            // TODO: check exception type
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate {
                ReferenceHost.Execute("$a.Bar = 0");
            });
        }

        [Test]
        public void InvokingMemberOfNullThrows()
        {
            // TODO: check exception type
            Assert.Throws(Is.InstanceOf(typeof(Exception)), delegate {
                ReferenceHost.Execute("$a.GetType()");
            });
        }

        [Test]
        public void AccessingMemberOfNullDoesntThrow()
        {
            Assert.DoesNotThrow(delegate {
                ReferenceHost.Execute("$null.Foo; $a.Bar");
            });
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
        public void CanInvokeMethodWithNullArg()
        {
            ExecuteAndCompareTypedResult("[string]::IsNullOrEmpty($null)", true);
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

        [Test]
        public void ReadOnlyParameterizedPropertyCanBeAccessed()
        {
            string result = ReferenceHost.Execute(NewlineJoin(
                "$a = Test-CreateParameterizedPropertiesObject -ReadOnly -FileNames 'abc.txt'",
                "$a.FileNames(0)"
            ));

            Assert.AreEqual(NewlineJoin("abc.txt"), result);
        }

        [Test]
        public void ReadWriteParameterizedPropertyCanBeWrittenToAndReadFrom()
        {
            string result = ReferenceHost.Execute(NewlineJoin(
                "$a = Test-CreateParameterizedPropertiesObject -FileNames 'a.txt'",
                "$a.FileNames(0) = 'b.txt'",
                "$a.FileNames(0)"
            ));

            Assert.AreEqual(NewlineJoin("b.txt"), result);
        }

        [Test]
        public void OverloadedParameterizedPropertyCanBeAccessed()
        {
            string result = ReferenceHost.Execute(NewlineJoin(
                "$a = Test-CreateOverloadedByArgumentNumbersParameterizedPropertiesObject",
                "$a.Grid(1, 2)"
            ));

            Assert.AreEqual(NewlineJoin("1, 2"), result);
        }

        [Test]
        public void OverloadedParameterizedPropertyCanBeWrittenToAndReadFrom()
        {
            string result = ReferenceHost.Execute(NewlineJoin(
                "$a = Test-CreateOverloadedByArgumentNumbersParameterizedPropertiesObject",
                "$a.Grid(1, 2) = 'b.txt'",
                "$a.Grid(1, 2)"
            ));

            Assert.AreEqual(NewlineJoin("b.txt"), result);
        }

        [Test]
        public void InterfaceParameterizedPropertyCanBeWrittenToAndReadFrom()
        {
            string result = ReferenceHost.Execute(NewlineJoin(
                "$a = Test-CreateParameterizedPropertiesObject -FromInterface -FileNames 'a.txt'",
                "$a.FileNames(0) = 'b.txt'",
                "$a.FileNames(0)"
            ));

            Assert.AreEqual(NewlineJoin("b.txt"), result);
        }

        [Test]
        public void OverloadedByTypeParameterizedPropertyCanBeWrittenToAndReadUsingFirstOverload()
        {
            string result = ReferenceHost.Execute(NewlineJoin(
                "$a = Test-CreateParameterizedPropertiesObject -OverloadedByType -FileNames 'a.txt'",
                "$a.FileNames(0) = 'b.txt'",
                "$a.FileNames(0)"
            ));

            Assert.AreEqual(NewlineJoin("b.txt"), result);
        }

        [Test]
        public void OverloadedByTypeParameterizedPropertyCanBeWrittenToAndReadUsingSecondOverload()
        {
            string result = ReferenceHost.Execute(NewlineJoin(
                "$a = Test-CreateParameterizedPropertiesObject -OverloadedByType -FileNames 'a.txt'",
                "$a.FileNames('a.txt') = 'b.txt'",
                "$a.FileNames('b.txt')"
            ));

            Assert.AreEqual(NewlineJoin("1"), result);
        }

        [Test]
        public void OverloadedParameterizedPropertyWithDifferentReturnTypeCanBeReadFrom()
        {
            string result = ReferenceHost.Execute(NewlineJoin(
                "$a = Test-CreateParameterizedPropertiesObject -DifferentReturnType -FileNames 'a.txt','b.txt'",
                "$a.FileNames('b.txt')"
            ));

            Assert.AreEqual(NewlineJoin("1"), result);
        }

        public class XmlTests
        {
            [Test]
            public void CanGetValueFromRootXmlElement()
            {
                var result = ReferenceHost.Execute(NewlineJoin(
                    "$path = [xml]\"<a>hello a</a>\"",
                    "$path.a"
                ));
                Assert.AreEqual(NewlineJoin("hello a"), result);
            }

            [Test]
            public void CanGetValueFromChildXmlElement()
            {
                var result = ReferenceHost.Execute(NewlineJoin(
                    "$path = [xml]\"<a><b>hello a</b></a>\"",
                    "$path.a.b"
                ));
                Assert.AreEqual(NewlineJoin("hello a"), result);
            }

            [Test]
            public void ArrayOfTextNodeElement()
            {
                var result = ReferenceHost.Execute(NewlineJoin(
                    "$path = [xml]\"<a><b>hello a</b><b>hello a</b></a>\"",
                    "$path.a.b"
                ));
                Assert.AreEqual(NewlineJoin("hello a", "hello a"), result);
            }

            [Test]
            public void ArrayOfTextNodeSubElement()
            {
                var result = ReferenceHost.Execute(NewlineJoin(
                    "$path = [xml]\"<a><b><c>CCC1</c><d>DDD1</d></b><b><c>CCC2</c><d>DDD1</d></b></a>\"",
                    "$path.a.b.c"
                ));
                Assert.AreEqual(NewlineJoin("CCC1", "CCC2"), result);
            }

            [Test]
            public void MissingXmlProperty()
            {
                var result = ReferenceHost.Execute(NewlineJoin(
                    "$path = [xml]\"<a>hello a</a>\"",
                    "$path.z"
                ));
                Assert.AreEqual(NewlineJoin(), result);
            }
        }
    }
}

