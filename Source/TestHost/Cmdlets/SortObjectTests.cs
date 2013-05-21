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
    }
}
