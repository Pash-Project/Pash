// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.IO;

namespace TestHost.Cmdlets
{
    [TestFixture]
    public class JoinPathTests
    {
        [Test]
        public void OneParentFolderAndChildFolder()
        {
            string result = TestHost.Execute(@"Join-Path 'parent' 'child'");

            Assert.AreEqual(
                string.Format(@"parent{1}child{0}", Environment.NewLine, Path.DirectorySeparatorChar),
                result);
        }

        [Test]
        public void TwoParentFoldersAndOneChildFolder()
        {
            string result = TestHost.Execute(@"Join-Path parent1,parent2 child");

            Assert.AreEqual(
                string.Format(@"parent1{1}child{0}parent2{1}child{0}", Environment.NewLine, Path.DirectorySeparatorChar),
                result);
        }
    }
}
