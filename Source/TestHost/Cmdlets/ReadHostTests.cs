using System;
using NUnit.Framework;

namespace TestHost.Cmdlets
{
    [TestFixture]
    public class ReadHostTests
    {
        [Test]
        public void ReadString()
        {
            var ui = new TestHostUserInterface();
            string val = "foobar" + Environment.NewLine;
            ui.SetInput(val);
            var res = TestHost.Execute(true, null, ui, "Read-Host");
            // nl is after reading the input
            Assert.That(res, Is.EqualTo(Environment.NewLine + val));
        }

        [Test]
        public void ReadStringWithPrompt()
        {
            var ui = new TestHostUserInterface();
            string val = "foobar" + Environment.NewLine;
            ui.SetInput(val);
            var res = TestHost.Execute(true, null, ui, "Read-Host 'test'");
            // nl is after reading the input
            Assert.That(res, Is.EqualTo("test: " + Environment.NewLine + val));
        }

        [Test]
        public void ReadSecureString()
        {
            var ui = new TestHostUserInterface();
            string val = "foobar" + Environment.NewLine;
            ui.SetInput(val);
            var res = TestHost.Execute(true, null, ui,
                "$x = Read-Host -AsSecureString;" + TestUtil.PashDecodeSecureString.Call("$x"));
            // nl is after reading the input
            Assert.That(res, Is.EqualTo(Environment.NewLine + val));
        }

        [Test]
        public void ReadSecureStringWithPrompt()
        {
            var ui = new TestHostUserInterface();
            string val = "foobar" + Environment.NewLine;
            ui.SetInput(val);
            var cmd = "$x = Read-Host 'test' -AsSecureString;" + TestUtil.PashDecodeSecureString.Call("$x");
            var res = TestHost.Execute(true, null, ui, cmd);
            // first nl is after reading the input
            Assert.That(res, Is.EqualTo("test: " + Environment.NewLine + val));
        }
    }
}

