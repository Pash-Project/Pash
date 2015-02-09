// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.IO;

namespace ReferenceTests.Commands
{
    [TestFixture]
    public class SplitPathTests : ReferenceTestBase
    {
        [Test]
        public void OneParentFolder()
        {
            string result = ReferenceHost.Execute(string.Format("Split-Path 'parent{0}child'", Path.DirectorySeparatorChar));

            Assert.AreEqual("parent" + Environment.NewLine, result);
        }

        [Test]
        public void TwoParentFolders()
        {
            string result = ReferenceHost.Execute(string.Format("Split-Path parent1{0}child,parent2{0}child", Path.DirectorySeparatorChar));

            Assert.AreEqual(string.Format("parent1{0}parent2{0}", Environment.NewLine), result);
        }
    }
}
