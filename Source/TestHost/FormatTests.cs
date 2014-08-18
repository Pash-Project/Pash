using System;
using NUnit.Framework;
using Microsoft.PowerShell.Commands.Utility;
using Microsoft.SqlServer.Server;
using System.Management.Automation.Host;

namespace TestHost
{
    [TestFixture]
    public class FormatTests
    {

        [TestCase("Format-List", typeof(ListFormatEntryData))]
        [TestCase("Format-Table", typeof(TableFormatEntryData))]
        public void FormattingObjectsGeneratesDocumentStructure(string formatCmd, Type entryFormat)
        {
            var cmd = NewlineJoin(
                "$a = new-object psobject -property @{foo='bar'; bar=2}",
                "$fmt = $a,$a | " + formatCmd,
                "foreach ($f in $fmt) { $f.GetType().Name }"
            );
            var expected = NewlineJoin(
                typeof(FormatStartData).Name,
                typeof(GroupStartData).Name,
                entryFormat.Name,
                entryFormat.Name,
                typeof(GroupEndData).Name,
                typeof(FormatEndData).Name
            );
            var result = TestHost.Execute(cmd);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void EmptyPSObjectDoesntThrow()
        {
            Assert.DoesNotThrow(delegate {
                TestHost.ExecuteWithZeroErrors("New-Object psobject");
            });
        }

        [Test, Combinatorial]
        public void SimpleDataDoesntNeedDocumentStructure(
            [Values("1", "-1", "'foo'", "2147483647000")]
            string data,
            [Values("Format-Table", "Format-List")]
            string formatCmd
        )
        {
            var cmd = NewlineJoin(
                "$fmt = " + data + " | " + formatCmd,
                "$fmt.GetType().Name"
                );
            var expected = NewlineJoin(typeof(SimpleFormatEntryData).Name);
            var result = TestHost.Execute(cmd);
            Assert.AreEqual(expected, result);
        }

        [TestCase("Format-Table")]
        [TestCase("Format-List")]
        public void ExceptionDataIsSpecialFormat(string formatCmd)
        {
            var cmd = NewlineJoin(
                "$fmt = new-object exception | " + formatCmd,
                "$fmt.GetType().Name"
                );
            var expected = NewlineJoin(typeof(ErrorFormatEntryData).Name);
            var result = TestHost.Execute(cmd);
            Assert.AreEqual(expected, result);
        }

        // I know the following tests are critical and depend highly on the formatters itself, but we need to test
        // basic formatting

        [Test]
        public void SimpleFormatListWorks()
        {

            var cmd = "new-object psobject -property @{barbaz=2} | Format-List";
            var expected = NewlineJoin(
                "",
                "barbaz : 2",
                ""
                );
            var result = TestHost.Execute(true, null, new TestHostUserInterface(20, 100), cmd);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SimpleFormatTableWorks()
        {
            var cmd = "new-object psobject -property @{foo='bar'} | Format-Table";
            var expected = NewlineJoin(
                "",
                "foo".PadRight(19),
                "---".PadRight(19),
                "bar".PadRight(19),
                ""
                );
            var result = TestHost.Execute(true, null, new TestHostUserInterface(20, 100), cmd);
            Assert.AreEqual(expected, result);
        }

        [TestCase("$true", "True")]
        [TestCase("$false", "False")]
        [TestCase("1", "1")]
        [TestCase("-1", "-1")]
        [TestCase("2147483647000", "2147483647000")]
        public void NumbersAndBoolsAreRightAlignedInTable(string value, string strValue)
        {
            var cmd = "new-object psobject -property @{foo=" + value + "} | Format-Table";
            var expected = NewlineJoin(
                "",
                "foo".PadLeft(19),
                "---".PadLeft(19),
                strValue.PadLeft(19),
                ""
                );
            var result = TestHost.Execute(true, null, new TestHostUserInterface(20, 100), cmd);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void FirstRowInTableDeterminesAlignment()
        {
            var cmd = NewlineJoin(
                "$b = new-object psobject -property @{barbaz='foo'}",
                "$a = new-object psobject -property @{barbaz=2}",
                "$b, $a | Format-Table"
            );
            // second column is now left aligned because the first object contains a string
            // with an int it's right aligned (check above)
            var expected = NewlineJoin(
                "",
                "barbaz".PadRight(19),
                "------".PadRight(19),
                "foo".PadRight(19),
                "2".PadRight(19),
                ""
                );
            var result = TestHost.Execute(true, null, new TestHostUserInterface(20, 100), cmd);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void MultilineListIsCorrectlyFormatted()
        {
            var cmd = NewlineJoin(
                "$a = new-object psobject -property @{foo='bar'}",
                "$b = new-object psobject -property @{foobar='baz'}",
                "$a, $b | Format-List"
            );
            // second column is now left aligned because the first object contains a string
            // with an int it's right aligned (check above)
            var expected = NewlineJoin(
                "",
                "foo : bar",
                "",
                "foobar : baz",
                ""
                );
            var result = TestHost.Execute(true, null, new TestHostUserInterface(20, 100), cmd);
            Assert.AreEqual(expected, result);
        }


        [Test]
        public void MultilineTableShowsColumnsOfFirstObject()
        {
            var cmd = NewlineJoin(
                "$a = new-object psobject -property @{foo='bar'}",
                "$b = new-object psobject -property @{foobar='baz'}",
                "$a, $b | Format-Table"
            );
            // second column is now left aligned because the first object contains a string
            // with an int it's right aligned (check above)
            var expected = NewlineJoin(
                "",
                "foo".PadRight(19),
                "---".PadRight(19),
                "bar".PadRight(19),
                "".PadRight(19),
                ""
                );
            var result = TestHost.Execute(true, null, new TestHostUserInterface(20, 100), cmd);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void FormatCommandUnpacksRecursiveArrayByOneLevel()
        {
            var cmd = "@(1,2,@(3,4,@(5,6),7),8) | format-table";
            var result = TestHost.Execute(true, null, new TestHostUserInterface(150, 100), cmd);
            var linestarts = new [] {
                "1",
                "2",
                "3",
                "4",
                " ", // line with table headers
                " ", // line with header borders
                " ", // line with Array-object data
                "7",
                "8",
            };
            var reslines = result.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(linestarts.Length, reslines.Length);
            for (int i = 0; i < reslines.Length; i++)
            {
                StringAssert.StartsWith(linestarts[i], reslines[i]);
            }
        }

        private string NewlineJoin(params string[] strs)
        {
            return String.Join(Environment.NewLine, strs) + Environment.NewLine;
        }
    }
}

