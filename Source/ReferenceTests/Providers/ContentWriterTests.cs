// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ReferenceTests.Providers
{
    [TestFixture]
    public class ContentWriterTests : ReferenceTestBaseWithTestModule
    {
        [Test]
        public void WriteStringArrayToFileByDefaultAppendsText()
        {
            string fileName = CreateFile(NewlineJoin("first", "second"), ".txt");
            ReferenceHost.Execute("Test-ContentWriter -value 'abc','def' -path " + fileName);

            string text = File.ReadAllText(fileName);
            Assert.AreEqual(NewlineJoin("first", "second", "abc", "def"), text);
        }
    }
}
