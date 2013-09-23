// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;

namespace TestHost.Cmdlets
{
    [TestFixture]
    public class JoinPathTests
    {
        [Test]
        [Platform("Win")]
        public void OneParentFolderAndChildFolderUnderWindows()
        {
            string result = TestHost.Execute(@"Join-Path 'parent' 'child'");

            Assert.AreEqual(@"parent\child" + Environment.NewLine, result);
        }

        [Test]
        [Platform("Unix")]
        public void OneParentFolderAndChildFolderUnderUnix()
        {
            string result = TestHost.Execute(@"Join-Path 'parent' 'child'");

            Assert.AreEqual(@"parent/child" + Environment.NewLine, result);
        }

        [Test]
        [Platform("Win")]
        public void TwoParentFoldersAndOneChildFolderUnderWindows()
        {
            string result = TestHost.Execute(@"Join-Path parent1,parent2 child");

            Assert.AreEqual(string.Format(@"parent1\child{0}parent2\child{0}", Environment.NewLine), result);
        }

        [Test]
        [Platform("Unix")]
        public void TwoParentFoldersAndOneChildFolderUnderUnix()
        {
            string result = TestHost.Execute(@"Join-Path parent1,parent2 child");

            Assert.AreEqual(string.Format(@"parent1/child{0}parent2/child{0}", Environment.NewLine), result);
        }
    }
}
