// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Pash.Implementation;

namespace TestHost
{
    [TestFixture]
    public class LocalHostUserInterfaceTests
    {
        private TextReader originalIn;
        private TextWriter originalOut;
        private StringWriter currentOut;

        private void SetInput(string input)
        {
            Console.SetIn(new StringReader(input));
        }

        private string GetOutput ()
        {
            return currentOut.ToString();
        }

        [SetUp]
        public void SaveConsoleIn()
        {
            originalIn = Console.In;
            originalOut = Console.Out;
            currentOut = new StringWriter();
            Console.SetOut(currentOut);
        }

        [TearDown]
        public void RestoreConsoleIn()
        {
            currentOut.Close();
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }

        [Test]
        public void TestReadLine()
        {
            SetInput("foobar" + Environment.NewLine);
            var ui = new LocalHostUserInterface();

            Assert.AreEqual("foobar", ui.ReadLine());
        }

        [Test]
        public void TestWriteLine()
        {
            var ui = new LocalHostUserInterface();
            string str = "foobar";
            ui.WriteLine(str);
            Assert.AreEqual(str + Environment.NewLine, GetOutput());
        }

    }
}