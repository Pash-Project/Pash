using System;
using System.Diagnostics;
using NUnit.Framework;
using System.IO;
using Pash;
using System.Collections.Generic;

namespace TestHost
{
    [TestFixture]
    public class FullHostTests
    {
        private List<string> _tempFiles;
        internal TestHostUserInterface HostUI;
        internal FullHost FullHost;

        private void CreateFreshHostAndUI(bool interactive)
        {
            HostUI = new TestHostUserInterface();
            FullHost = new FullHost(interactive);
            FullHost.LocalHost.SetHostUserInterface(HostUI);
        }

        [SetUp]
        public void SetUp()
        {
            _tempFiles = new List<string>();
        }

        [TearDown]
        public void RemoveTempfiles()
        {
            foreach (var file in _tempFiles)
            {
                File.Delete(file);
            }
            _tempFiles.Clear();
        }

        [Test]
        public void RunAndInteractiveExit()
        {
            CreateFreshHostAndUI(true);
            HostUI.SetInput("exit");
            FullHost.Run();
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, outlines.Length); // banner and prompt
            outlines.ShouldContain(FullHost.BannerText);
        }

        [Test]
        public void RunAndNoInteraction()
        {
            CreateFreshHostAndUI(false);
            HostUI.SetInput("exit"); // just in case, so the test case exits
            FullHost.Run(null);
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, outlines.Length); // banner only, no prompt because of autoexit
            outlines.ShouldContain(FullHost.BannerText);
        }

        [Test]
        public void RunWithScriptAndInteractiveInput()
        {
            CreateFreshHostAndUI(true);
            HostUI.SetInput("exit");
            FullHost.Run(@"Write-Host ""foo""; Write-Host ""bar"";");
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(3, outlines.Length); // 2x output of command, and prompt
            outlines.ShouldContain("foo");
            outlines.ShouldContain("bar");
        }

        [Test]
        public void RunWithScriptAndNoInteraction()
        {
            CreateFreshHostAndUI(false);
            HostUI.SetInput("exit"); // just in case, so the test case exits
            FullHost.Run(@"Write-Host ""foo""; Write-Host ""bar"";");
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, outlines.Length); // 2x output of command, no prompt
            outlines.ShouldContain("foo");
            outlines.ShouldContain("bar");
        }

        [Test]
        public void LoadProfileWorks()
        {
            CreateFreshHostAndUI(false);
            HostUI.SetInput("exit"); // just in case
            var tmpfile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".ps1");
            File.Create(tmpfile).Close();
            _tempFiles.Add(tmpfile);
            File.WriteAllText(tmpfile, "function myfun { Write-Host 'test'; }");
            FullHost.LoadProfile(tmpfile);
            FullHost.Run("myfun"); // this should exist now
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, outlines.Length);
            Assert.AreEqual("test", outlines[0]);
        }

        [Test]
        public void ProfileVariableGetsSetCorrectly()
        {
            CreateFreshHostAndUI(false);
            HostUI.SetInput("exit"); // just in case
            FullHost.SetProfileVariable("cuch", "cuah", "auch", "auah");
            FullHost.Run("$profile; $profile.CurrentUserCurrentHost; $profile.CurrentUserAllHosts; "
                        + "$profile.AllUsersCurrentHost; $profile.AllUsersAllHosts;");
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(5, outlines.Length);
            Assert.AreEqual("cuch", outlines[0]);
            Assert.AreEqual("cuch", outlines[1]);
            Assert.AreEqual("cuah", outlines[2]);
            Assert.AreEqual("auch", outlines[3]);
            Assert.AreEqual("auah", outlines[4]);
        }

        [Test]
        [Timeout(3000)] // just in case the test runs into a loop
        public void CanParseTakePartialInput()
        {
            CreateFreshHostAndUI(true);
            HostUI.SetInput(String.Join(Environment.NewLine, new [] {
                "if ($true)",
                "{",
                "'foo'",
                "}"
            }) + Environment.NewLine + Environment.NewLine); // last newline "confirms" that partial parsing can end
            FullHost.Run();
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(8, outlines.Length); // Banner + 5 x Prompts (per input line) + result + new prompt
            Assert.That(outlines[6], Is.EqualTo("foo"));
        }

        [Test]
        public void ParseErrorOnInputIsPrinted()
        {
            CreateFreshHostAndUI(true);
            HostUI.OnWriteErrorLineString = s => HostUI.WriteLine(s); // we want to see errors in output
            HostUI.SetInput("$" + Environment.NewLine); // simple parse error
            FullHost.Run();
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(6, outlines.Length); // Banner + prompt + 3 error lines + prompt
            Assert.That(outlines[2], Is.StringStarting("Parse error"));
        }
    }
}

