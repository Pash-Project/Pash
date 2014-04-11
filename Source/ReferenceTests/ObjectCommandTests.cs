using System;
using NUnit.Framework;
using System.Management.Automation;

namespace ReferenceTests
{
    [TestFixture]
    public class ObjectCommandTests : ReferenceTestBase
    {

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
    }
}

