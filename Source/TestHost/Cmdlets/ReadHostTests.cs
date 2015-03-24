using System;
using NUnit.Framework;

namespace TestHost
{
    [TestFixture]
    public class ReadHostTests
    {
        private const string _decodeSecureStrFun =
@"
            function secureStr2Str($secureStr) {
              $ptr = [System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($secureStr)
              $result = [System.Runtime.InteropServices.Marshal]::PtrToStringUni($ptr)
              [System.Runtime.InteropServices.Marshal]::ZeroFreeCoTaskMemUnicode($ptr)
              return $result;
            }
";

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
                _decodeSecureStrFun + "$x = Read-Host -AsSecureString; secureStr2Str($x)");
            // nl is after reading the input
            Assert.That(res, Is.EqualTo(Environment.NewLine + val));
        }

        [Test]
        public void ReadSecureStringWithPrompt()
        {
            var ui = new TestHostUserInterface();
            string val = "foobar" + Environment.NewLine;
            ui.SetInput(val);
            var res = TestHost.Execute(true, null, ui, "Read-Host 'test'");
            // first nl is after reading the input
            Assert.That(res, Is.EqualTo("test: " + Environment.NewLine + val));
        }
    }
}

