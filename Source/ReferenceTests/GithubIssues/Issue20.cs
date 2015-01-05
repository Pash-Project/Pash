using System;
using NUnit.Framework;

namespace ReferenceTests.GithubIssues
{
    [TestFixture]
    public class Issue20 : ReferenceTestBase
    {
        [Test]
        public void Issue20_UnrollExample1()
        {
            ExecuteAndCompareTypedResult("&{ @(1, 2), 3, 4} | % { \"'$_'\" }", "'1 2'", "'3'", "'4'");
        }

        [Test]
        public void Issue20_UnrollExample2()
        {
            ExecuteAndCompareTypedResult("&{ @(1, 2, 3, 4) } | % { \"'$_'\" }", "'1'", "'2'", "'3'", "'4'");
        }

        [Test]
        public void Issue20_UnrollExample3()
        {
            ExecuteAndCompareTypedResult("&{ @(1, 2); 3; 4 } | % { \"'$_'\" }", "'1'", "'2'", "'3'", "'4'");
        }

        [Test]
        public void Issue20_UnrollDictionary()
        {
            var cmd = NewlineJoin(
                "$sd = New-Object System.Collections.Specialized.StringDictionary",
                "$sd.Add(\"One\",\"Uno\"); $sd.Add(\"Two\",\"Dos\")",
                "$sd | % { \"'$_'\" }"
                );
            ExecuteAndCompareTypedResult(cmd, "'System.Collections.DictionaryEntry'", 
                                         "'System.Collections.DictionaryEntry'");
        }

        [Test]
        public void Issue20_HashtableIsNotUnrolled()
        {
            ExecuteAndCompareTypedResult("@{a=2;b=3} | % { \"'$_'\" }", "'System.Collections.Hashtable'");
        }
    }
}

