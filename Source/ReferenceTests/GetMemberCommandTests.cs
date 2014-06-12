using System;
using System.Linq;
using NUnit.Framework;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace ReferenceTests
{
    [TestFixture]
    public class GetMemberCommandTests : ReferenceTestBase
    {
        [Test]
        public void GetMemberWorks()
        {
            var psobj = PSObject.AsPSObject("1");
            var typename = typeof(string).FullName;
            var expected = (from info in psobj.Members.Match("GetType") select info.Name).ToList();
            var members = ReferenceHost.Execute("1 | Get-Member -name GetType | foreach-object {$_.name}")
                .Split(new[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(expected.Count, members.Length);
            foreach (var mem in members)
            {
                Assert.True(expected.Contains(mem));
            }
        }    
    }
}

