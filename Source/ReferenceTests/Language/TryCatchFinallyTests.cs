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

        [Test]
        public void TryCatchFinally()
        {
            ExecuteAndCompareTypedResult(
                "try { 2 + 2 } catch { -1 } finally { 0 }",
                4,
                0
            );
        }

        [Test]
        public void TryCatchFinallyWithException()
        {
            ExecuteAndCompareTypedResult(
                "try { 2 / 0 } catch { -1 } finally { 3 }",
                -1,
                3
            );
        }
    }
}

