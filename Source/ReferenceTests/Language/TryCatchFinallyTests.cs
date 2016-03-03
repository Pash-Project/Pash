using System;
using NUnit.Framework;

namespace ReferenceTests.Language
{
    [TestFixture]
    public class TryCatchFinallyTests : ReferenceTestBase
    {
        [Test]
        public void TryFinallyWithoutCatch()
        {
            ExecuteAndCompareTypedResult(
                "try { 2 + 2 } finally { 0 }",
                4,
                0
            );                
        }
    }
}

