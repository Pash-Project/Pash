using System;
using NUnit.Framework;
using System.Management.Automation;
using ReferenceTests;

namespace TestHost.Cmdlets
{
    [TestFixture]
    public class GetCredentialTests
    {
        [Test]
        public void GetCredential()
        {
            var ui = new TestHostUserInterface();
            var val = "testUser" + Environment.NewLine + "testPassword" + Environment.NewLine;
            ui.SetInput(val);

            var cmd = "$a = Get-Credential; $a.GetType().FullName;"
                    + "$a.UserName; " + TestUtil.PashDecodeSecureString.Call("$a.Password");
            var res = TestHost.Execute(true, null, ui, cmd);

            var lines = res.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines[lines.Length - 3], Is.EqualTo(typeof(PSCredential).FullName));
            Assert.That(lines[lines.Length - 2], Is.EqualTo("testUser"));
            Assert.That(lines[lines.Length - 1], Is.EqualTo("testPassword"));
        }

        [Test]
        public void GetCredentialWithPredefinedUsernameAsCredential()
        {
            var ui = new TestHostUserInterface();
            var val = Environment.NewLine + "testPassword" + Environment.NewLine;
            ui.SetInput(val);

            var cmd = "$a = Get-Credential -Credential 'defUser'; $a.GetType().FullName;"
                + "$a.UserName; " + TestUtil.PashDecodeSecureString.Call("$a.Password");
            var res = TestHost.Execute(true, null, ui, cmd);

            var lines = res.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines[lines.Length - 3], Is.EqualTo(typeof(PSCredential).FullName));
            Assert.That(lines[lines.Length - 2], Is.EqualTo("defUser"));
            Assert.That(lines[lines.Length - 1], Is.EqualTo("testPassword"));
        }

        [Test]
        public void GetCredentialWithUsernameAndMessage()
        {
            var ui = new TestHostUserInterface();
            var val = Environment.NewLine + "testPassword" + Environment.NewLine;
            ui.SetInput(val);

            var cmd = "$a = Get-Credential -Message 'testMessage' 'defUser'; $a.GetType().FullName;"
                + "$a.UserName; " + TestUtil.PashDecodeSecureString.Call("$a.Password");
            var res = TestHost.Execute(true, null, ui, cmd);

            var lines = res.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines, Contains.Item("testMessage"));
            Assert.That(lines[lines.Length - 3], Is.EqualTo(typeof(PSCredential).FullName));
            Assert.That(lines[lines.Length - 2], Is.EqualTo("defUser"));
            Assert.That(lines[lines.Length - 1], Is.EqualTo("testPassword"));
        }
    }
}

