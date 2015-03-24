using System;
using NUnit.Framework;
using System.Management.Automation.Runspaces;
using TestPSSnapIn;
using System.Management.Automation;

namespace TestHost
{
   [TestFixture]
    public class CmdletUserInputTests
    {
        [TearDown]
        public void ResetInitialSessionState()
        {
            //necessarry as TestHost is (unfortunately) used in a static way
            TestHost.InitialSessionState = null;
        }

        [SetUp]
        public void InitSessionState()
        {
            var ss = InitialSessionState.CreateDefault();
            ss.ImportPSModule(new string[] { typeof(TestWithMandatoryCommand).Assembly.Location });
            TestHost.InitialSessionState = ss;
        }

        private static string CmdletName(Type cmdletType)
        {
            var attribute = System.Attribute.GetCustomAttribute(cmdletType, typeof(CmdletAttribute))
                as CmdletAttribute;
            return string.Format("{0}-{1}", attribute.VerbName, attribute.NounName);
        }


        [Test]
        public void MandatoryValueIsGatheredIfNotProvided()
        {
            var ui = new TestHostUserInterface();
            string val = "foobar";
            ui.SetInput(val + Environment.NewLine);
            var res = TestHost.Execute(true, null, ui, CmdletName(typeof(TestWithMandatoryCommand)));
            var lines = res.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(TestWithMandatoryCommand.Transform(val), lines[lines.Length - 1]);
        }

        [Test]
        public void ValueGatheringCanProvideHelp()
        {
            var ui = new TestHostUserInterface();
            string val = "test";
            string input = "!?" + Environment.NewLine + val + Environment.NewLine;
            ui.SetInput(input);
            var res = TestHost.Execute(true, null, ui, CmdletName(typeof(TestWithMandatoryCommand)));
            var lines = res.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Greater(lines.Length, 3, "Not enough output");
            Assert.AreEqual(TestWithMandatoryCommand.Transform(val), lines[lines.Length - 1]);
            Assert.AreEqual(TestWithMandatoryCommand.HELP_MSG, lines[lines.Length - 3]);
        }

        [Test]
        public void ValueGatheringWithArrayInputAndConversion()
        {
            var ui = new TestHostUserInterface();
            var intArray = new [] { 4, 5, 6 };
            ui.SetInput(String.Join(Environment.NewLine, intArray) + Environment.NewLine + Environment.NewLine);
            var res = TestHost.Execute(true, null, ui, CmdletName(typeof(TestIntegerArraySumCommand)));
            var lines = res.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(TestIntegerArraySumCommand.Transform(intArray), lines[lines.Length - 1]);
        }

        [Test]
        public void ValueGatheringForPSCredential()
        {
            var ui = new TestHostUserInterface();
            ui.SetInput("TheUser" + Environment.NewLine + "SecretPassword" + Environment.NewLine);
            var res = TestHost.Execute(true, null, ui, CmdletName(typeof(TestPrintCredentialsCommand)));
            var lines = res.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines[lines.Length - 2], Is.EqualTo("User: TheUser"));
            Assert.That(lines[lines.Length - 1], Is.EqualTo("Password: SecretPassword"));
        }

        [Test]
        public void ValueGatheringForPSCredentialWithStringAsCredential()
        {
            var ui = new TestHostUserInterface();
            ui.SetInput("Bo" + Environment.NewLine + "SecretPassword" + Environment.NewLine);
            var res = TestHost.Execute(true, null, ui, CmdletName(typeof(TestPrintCredentialsCommand)) + " 'BiBa'");
            var lines = res.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines[lines.Length - 2], Is.EqualTo("User: BiBaBo"));
            Assert.That(lines[lines.Length - 1], Is.EqualTo("Password: SecretPassword"));
        }
    }
}

