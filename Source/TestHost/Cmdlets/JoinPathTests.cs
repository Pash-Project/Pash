// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;

namespace TestHost.Cmdlets
{
    [TestFixture]
    public class JoinPathTests
    {
        [Test]
        public void OneParentFolderAndChildFolder()
        {
            string result = TestHost.Execute(@"Join-Path 'parent' 'child'");

            Assert.AreEqual(@"parent\child" + Environment.NewLine, result);
        }

        [Test]
        public void TwoParentFoldersAndOneChildFolder()
        {
            string result = TestHost.Execute(@"Join-Path parent1,parent2 child");

            Assert.AreEqual(string.Format(@"parent1\child{0}parent2\child{0}", Environment.NewLine), result);
        }
    }
}
