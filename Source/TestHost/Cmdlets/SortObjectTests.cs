// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHost.Cmdlets
{
    [TestFixture]
    public class SortObjectTests
    {
        [Test]
        public void SortObjectSortsAscendingWithoutProperty()
        {
            var results = TestHost.Execute("3,1,9 | Sort-Object");
            Assert.AreEqual(string.Format("1{0}3{0}9{0}", Environment.NewLine), results);
        }

        [Test]
        public void SortObjectSortsDescendingWithoutProperty()
        {
            var results = TestHost.Execute("3,1,9 | Sort-Object -Desc");
            Assert.AreEqual(string.Format("9{0}3{0}1{0}", Environment.NewLine), results);
        }

        [Test]
        public void SortObjectSortsAscendingWithProperty()
        {
            var results = TestHost.Execute("'abcd','abcdefghi','a','ab' | Sort-Object Length");
            Assert.AreEqual(string.Format("a{0}ab{0}abcd{0}abcdefghi{0}", Environment.NewLine), results);
        }

        [Test]
        public void SortObjectSortsDescendingWithProperty()
        {
            var results = TestHost.Execute("'abcd','abcdefghi','a','ab' | Sort-Object -Desc Length");
            Assert.AreEqual(string.Format("abcdefghi{0}abcd{0}ab{0}a{0}", Environment.NewLine), results);
        }

        [Test]
        public void SortObjectUniqueAscending()
        {
            var results = TestHost.Execute("4,7,12,3,9,1,4,3 | Sort-Object -Unique");
            Assert.AreEqual(string.Format("1{0}3{0}4{0}7{0}9{0}12{0}", Environment.NewLine), results);
        }

        [Test]
        public void SortObjectUniqueDescending()
        {
            var results = TestHost.Execute("4,7,12,3,9,1,4,3 | Sort-Object -Unique -Desc");
            Assert.AreEqual(string.Format("12{0}9{0}7{0}4{0}3{0}1{0}", Environment.NewLine), results);
        }
    }
}
