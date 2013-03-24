// Copyright (C) Pash Contributors. License: GPL/BSD. See https://github.com/Pash-Project/Pash/
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spikes
{
    [TestFixture]
    public class MemberExpressionSpikes
    {
        [Test]
        public void ATest()
        {
            object o = "abc";
            string memberName = "Length";

            var result = o.GetType().GetProperty(memberName).GetValue(o, null);

            Assert.AreEqual(3, result);            
        }
    }
}
