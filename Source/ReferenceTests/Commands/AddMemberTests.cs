using System;
using NUnit.Framework;
using System.Management.Automation;
using TestPSSnapIn;
using System.Runtime.Remoting;
using System.Collections.Generic;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class AddMemberTests : ReferenceTestBase
    {
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
    }
}

