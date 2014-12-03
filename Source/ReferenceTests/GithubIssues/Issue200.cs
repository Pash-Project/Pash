using System;
using NUnit.Framework;

namespace ReferenceTests.GithubIssues
{
    [TestFixture]
    public class Issue200 : ReferenceTestBase
    {
        [Test]
        public void Issue200_StringIndexEquality()
        {
            ExecuteAndCompareTypedResult("$a = 'Hello'; if( $a[0] -eq 'H') { 'YES'; } else { 'NO'; }", "YES");
        }
    }
}

