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

        [SetUp]
        public void SaveConsoleIn ()
        {
            originalIn = Console.In;
            Console.SetWindowSize(60, 60);
        }

        [TearDown]
        public void RestoreConsoleIn ()
        {
            Console.SetIn(originalIn);
        }

        [Test]
        public void TestStdin()
        {
            Console.SetIn(new StringReader("foobar"));
            var ui = new LocalHostUserInterface();

            Assert.AreEqual("foobar", ui.ReadLine());
        }

    }
}