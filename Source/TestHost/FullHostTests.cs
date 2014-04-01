using System;
using System.Diagnostics;
using NUnit.Framework;
using System.IO;
using Pash;

namespace TestHost
{
    [TestFixture]
    public class FullHostTests
    {
        internal TestHostUserInterface HostUI;
        internal FullHost FullHost;

        private void CreateFreshHostAndUI(bool interactive)
        {
            HostUI = new TestHostUserInterface();
            FullHost = new FullHost(interactive);
            FullHost.LocalHost.SetHostUserInterface(HostUI);
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
    }
}

