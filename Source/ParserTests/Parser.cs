using System;
using NUnit.Framework;

namespace ParserTest
{
    [TestFixture]
    public class Parser
    {
        [Test]
        public void PassTest()
        {
        }

        [Test]
        public void FailTest()
        {
            throw new Exception();
        }
    }
}

