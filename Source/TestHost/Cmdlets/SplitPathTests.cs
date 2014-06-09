// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;

namespace TestHost.Cmdlets
{
    [TestFixture]
    public class SplitPathTests
    {
        [Test]
        [Platform("Win")]
        public void OneParentFolderUnderWindows()
        {
            string result = TestHost.Execute(@"Split-Path 'parent\child'");

            Assert.AreEqual(@"parent" + Environment.NewLine, result);
        }

        [Test]
        [Platform("Unix")]
        public void OneParentFolderUnderUnix()
        {
            string result = TestHost.Execute(@"Split-Path 'parent/child'");

            Assert.AreEqual(@"parent" + Environment.NewLine, result);
        }

        [Test]
        [Platform("Win")]
        public void TwoParentFoldersUnderWindows()
        {
            string result = TestHost.Execute(@"Split-Path parent1\child,parent2\child");

            Assert.AreEqual(string.Format(@"parent1{0}parent2{0}", Environment.NewLine), result);
        }

        [Test]
        [Platform("Unix")]
        public void TwoParentFoldersUnderUnix()
        {
            string result = TestHost.Execute(@"Split-Path parent1/child,parent2/child");

            Assert.AreEqual(string.Format(@"parent1{0}parent2{0}", Environment.NewLine), result);
        }
    }
}
