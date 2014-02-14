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

        [SetUp]
        public void CreateFreshHostAndUI()
        {
            HostUI = new TestHostUserInterface();
            FullHost = new FullHost();
            FullHost.LocalHost.SetHostUserInterface(HostUI);
        }

        [Test]
        public void RunAndInteractiveExit()
        {
            HostUI.SetInput("exit");
            FullHost.Run();
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, outlines.Length); // banner and prompt
            outlines.ShouldContain(FullHost.BannerText);
        }

        [Test]
        public void RunAndNoInteraction()
        {
            HostUI.SetInput("exit"); // just in case, so the test case exits
            FullHost.Run(false, null);
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, outlines.Length); // banner only, no prompt because of autoexit
            outlines.ShouldContain(FullHost.BannerText);
        }

        [Test]
        public void RunWithScriptAndInteractiveInput()
        {
            HostUI.SetInput("exit");
            FullHost.Run(true, @"Write-Host ""foo""; Write-Host ""bar"";");
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(3, outlines.Length); // 2x output of command, and prompt
            outlines.ShouldContain("foo");
            outlines.ShouldContain("bar");
        }

        [Test]
        public void RunWithScriptAndNoInteraction()
        {
            HostUI.SetInput("exit"); // just in case, so the test case exits
            FullHost.Run(false, @"Write-Host ""foo""; Write-Host ""bar"";");
            var outlines = HostUI.GetOutput().Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, outlines.Length); // 2x output of command, no prompt
            outlines.ShouldContain("foo");
            outlines.ShouldContain("bar");
        }
    }
}

